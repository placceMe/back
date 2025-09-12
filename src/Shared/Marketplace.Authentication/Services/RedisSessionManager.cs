using Marketplace.Authentication.Abstractions;
using Marketplace.Authentication.Configuration;
using Marketplace.Authentication.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Marketplace.Authentication.Services;

public class RedisSessionManager : ISessionManager
{
    private readonly IDatabase _database;
    private readonly IServer _server;
    private readonly JwtTokenService _jwtService;
    private readonly AuthenticationOptions _options;
    private readonly ILogger<RedisSessionManager> _logger;

    public RedisSessionManager(
        IConnectionMultiplexer redis,
        JwtTokenService jwtService,
        IOptions<AuthenticationOptions> options,
        ILogger<RedisSessionManager> logger)
    {
        _options = options.Value;
        _database = redis.GetDatabase(_options.Redis.DefaultDatabaseIndex);
        _server = redis.GetServer(redis.GetEndPoints().First());
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<SessionToken> CreateSessionAsync(CreateSessionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // ????????? ?????
            var session = new AuthSession
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                UserEmail = request.UserEmail,
                UserName = request.UserName,
                Roles = request.Roles,
                DeviceId = request.DeviceId,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(request.SessionDuration),
                IsActive = true,
                Metadata = request.Metadata ?? new Dictionary<string, object>()
            };

            // ????????? ??????
            var accessToken = _jwtService.GenerateAccessToken(session);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var sessionToken = new SessionToken
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_options.Jwt.AccessTokenExpiryMinutes),
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(_options.Jwt.RefreshTokenExpiryDays),
                SessionId = session.Id.ToString()
            };

            // ?????????? ? Redis
            await SaveSessionAsync(session, sessionToken, cancellationToken);

            // ???????? ????? ????? ???? ?????????? ?????
            await EnforceSessionLimitAsync(request.UserId, cancellationToken);

            _logger.LogInformation("Created new session {SessionId} for user {UserId}", session.Id, request.UserId);
            return sessionToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create session for user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<SessionValidationResult> ValidateSessionAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // ????????? JWT ?????
            var principal = _jwtService.ValidateToken(accessToken);
            if (principal == null)
            {
                return SessionValidationResult.Failed("Invalid access token");
            }

            // ????????? session ID ? ??????
            var sessionId = _jwtService.GetSessionIdFromToken(accessToken);
            if (string.IsNullOrEmpty(sessionId))
            {
                return SessionValidationResult.Failed("Session ID not found in token");
            }

            // ??????????? ?? ????? ?? ??????????
            var jti = _jwtService.GetJtiFromToken(accessToken);
            if (!string.IsNullOrEmpty(jti) && await IsTokenRevokedAsync(jti, cancellationToken))
            {
                return SessionValidationResult.Failed("Token has been revoked");
            }

            return await ValidateSessionByIdAsync(sessionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate session token");
            return SessionValidationResult.Failed("Session validation error");
        }
    }

    public async Task<SessionValidationResult> ValidateSessionByIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await GetSessionAsync(sessionId, cancellationToken);
            if (session == null)
            {
                return SessionValidationResult.Failed("Session not found");
            }

            if (!session.IsActive)
            {
                return SessionValidationResult.Failed("Session is inactive");
            }

            if (session.ExpiresAt <= DateTime.UtcNow)
            {
                await InvalidateSessionAsync(sessionId, cancellationToken);
                return SessionValidationResult.Expired();
            }

            // ??????????? ?? ???????? ??????? ??????????
            var timeSinceActivity = DateTime.UtcNow - session.LastActivity;
            if (timeSinceActivity > _options.Session.SessionExtensionThreshold)
            {
                await UpdateSessionActivityAsync(sessionId, cancellationToken);
                session.LastActivity = DateTime.UtcNow;
            }

            // ??????????? ?? ???????? refresh
            var timeToExpiry = session.ExpiresAt - DateTime.UtcNow;
            if (timeToExpiry <= _options.Session.RefreshThreshold)
            {
                return SessionValidationResult.RefreshRequired(session);
            }

            return SessionValidationResult.Success(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate session {SessionId}", sessionId);
            return SessionValidationResult.Failed("Session validation error");
        }
    }

    public async Task<bool> RefreshSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await GetSessionAsync(sessionId, cancellationToken);
            if (session == null || !session.IsActive)
            {
                return false;
            }

            // ??????????? ?????
            session.ExpiresAt = DateTime.UtcNow.Add(_options.Session.DefaultSessionDuration);
            session.LastActivity = DateTime.UtcNow;

            await SaveSessionAsync(session, null, cancellationToken);
            
            _logger.LogDebug("Refreshed session {SessionId}", sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> InvalidateSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var batch = _database.CreateBatch();
            
            // ????????? ?????
            var sessionKey = GetSessionKey(sessionId);
            var task1 = batch.KeyDeleteAsync(sessionKey);
            
            // ????????? refresh token
            var refreshTokenKey = GetRefreshTokenKey(sessionId);
            var task2 = batch.KeyDeleteAsync(refreshTokenKey);
            
            batch.Execute();
            
            await Task.WhenAll(task1, task2);
            
            _logger.LogInformation("Invalidated session {SessionId}", sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invalidate session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> InvalidateAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await GetUserSessionsAsync(userId, cancellationToken);
            var batch = _database.CreateBatch();
            var tasks = new List<Task>();

            foreach (var session in sessions)
            {
                var sessionKey = GetSessionKey(session.Id.ToString());
                var refreshTokenKey = GetRefreshTokenKey(session.Id.ToString());
                
                tasks.Add(batch.KeyDeleteAsync(sessionKey));
                tasks.Add(batch.KeyDeleteAsync(refreshTokenKey));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Invalidated all sessions for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invalidate all sessions for user {UserId}", userId);
            return false;
        }
    }

    public async Task<List<AuthSession>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var pattern = GetUserSessionPattern(userId);
            var keys = _server.Keys(pattern: pattern);
            
            var sessions = new List<AuthSession>();
            
            foreach (var key in keys)
            {
                var sessionData = await _database.StringGetAsync(key);
                if (sessionData.HasValue)
                {
                    var session = JsonSerializer.Deserialize<AuthSession>(sessionData!);
                    if (session != null && session.UserId == userId)
                    {
                        sessions.Add(session);
                    }
                }
            }
            
            return sessions.OrderByDescending(s => s.LastActivity).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sessions for user {UserId}", userId);
            return new List<AuthSession>();
        }
    }

    public async Task<SessionToken?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // ????????? ????? ?? refresh token
            var sessionId = await FindSessionByRefreshTokenAsync(refreshToken, cancellationToken);
            if (string.IsNullOrEmpty(sessionId))
            {
                return null;
            }

            var session = await GetSessionAsync(sessionId, cancellationToken);
            if (session == null || !session.IsActive || session.ExpiresAt <= DateTime.UtcNow)
            {
                return null;
            }

            // ????????? ???? ??????
            var newAccessToken = _jwtService.GenerateAccessToken(session);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            var sessionToken = new SessionToken
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_options.Jwt.AccessTokenExpiryMinutes),
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(_options.Jwt.RefreshTokenExpiryDays),
                SessionId = session.Id.ToString()
            };

            // ????????? ?????????? ?????
            session.LastActivity = DateTime.UtcNow;
            await SaveSessionAsync(session, sessionToken, cancellationToken);

            _logger.LogDebug("Refreshed tokens for session {SessionId}", sessionId);
            return sessionToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token");
            return null;
        }
    }

    public async Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var jti = _jwtService.GetJtiFromToken(token);
            if (string.IsNullOrEmpty(jti))
            {
                return false;
            }

            var expiry = _jwtService.GetTokenExpiry(token);
            var ttl = expiry - DateTime.UtcNow;
            
            if (ttl > TimeSpan.Zero)
            {
                var revokedTokenKey = GetRevokedTokenKey(jti);
                await _database.StringSetAsync(revokedTokenKey, "revoked", ttl);
                
                _logger.LogDebug("Revoked token {Jti}", jti);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke token");
            return false;
        }
    }

    public async Task<bool> IsTokenRevokedAsync(string tokenId, CancellationToken cancellationToken = default)
    {
        try
        {
            var revokedTokenKey = GetRevokedTokenKey(tokenId);
            return await _database.KeyExistsAsync(revokedTokenKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if token is revoked");
            return false;
        }
    }

    public async Task<bool> UpdateSessionActivityAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await GetSessionAsync(sessionId, cancellationToken);
            if (session == null)
            {
                return false;
            }

            session.LastActivity = DateTime.UtcNow;
            
            // ??????????? ??????????? ????? ???? ?????????
            if (_options.Session.EnableSessionExtension)
            {
                session.ExpiresAt = DateTime.UtcNow.Add(_options.Session.DefaultSessionDuration);
            }

            await SaveSessionAsync(session, null, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update session activity for {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> UpdateSessionMetadataAsync(string sessionId, Dictionary<string, object> metadata, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await GetSessionAsync(sessionId, cancellationToken);
            if (session == null)
            {
                return false;
            }

            session.Metadata = metadata;
            session.LastActivity = DateTime.UtcNow;

            await SaveSessionAsync(session, null, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update session metadata for {SessionId}", sessionId);
            return false;
        }
    }

    public async Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var pattern = $"{_options.Redis.KeyPrefix}session:*";
            var keys = _server.Keys(pattern: pattern);
            var expiredCount = 0;

            foreach (var key in keys)
            {
                var sessionData = await _database.StringGetAsync(key);
                if (sessionData.HasValue)
                {
                    var session = JsonSerializer.Deserialize<AuthSession>(sessionData!);
                    if (session?.ExpiresAt <= DateTime.UtcNow)
                    {
                        await InvalidateSessionAsync(session.Id.ToString(), cancellationToken);
                        expiredCount++;
                    }
                }
            }

            if (expiredCount > 0)
            {
                _logger.LogInformation("Cleaned up {ExpiredCount} expired sessions", expiredCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired sessions");
        }
    }

    // Private helper methods
    private async Task<AuthSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var sessionKey = GetSessionKey(sessionId);
        var sessionData = await _database.StringGetAsync(sessionKey);
        
        if (!sessionData.HasValue)
        {
            return null;
        }

        return JsonSerializer.Deserialize<AuthSession>(sessionData!);
    }

    private async Task SaveSessionAsync(AuthSession session, SessionToken? sessionToken, CancellationToken cancellationToken = default)
    {
        var sessionKey = GetSessionKey(session.Id.ToString());
        var sessionData = JsonSerializer.Serialize(session);
        var ttl = session.ExpiresAt - DateTime.UtcNow;

        var batch = _database.CreateBatch();
        
        // ?????????? ?????
        var task1 = batch.StringSetAsync(sessionKey, sessionData, ttl);
        
        // ?????????? refresh token ???? ?
        Task? task2 = null;
        if (sessionToken != null)
        {
            var refreshTokenKey = GetRefreshTokenKey(session.Id.ToString());
            var refreshTtl = sessionToken.RefreshTokenExpiry - DateTime.UtcNow;
            task2 = batch.StringSetAsync(refreshTokenKey, sessionToken.RefreshToken, refreshTtl);
        }

        batch.Execute();
        
        if (task2 != null)
        {
            await Task.WhenAll(task1, task2);
        }
        else
        {
            await task1;
        }
    }

    private async Task<string?> FindSessionByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var pattern = $"{_options.Redis.KeyPrefix}refresh:*";
        var keys = _server.Keys(pattern: pattern);

        foreach (var key in keys)
        {
            var storedToken = await _database.StringGetAsync(key);
            if (storedToken == refreshToken)
            {
                // ????????? sessionId ? ?????
                var keyStr = key.ToString();
                return keyStr.Substring(keyStr.LastIndexOf(':') + 1);
            }
        }

        return null;
    }

    private async Task EnforceSessionLimitAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await GetUserSessionsAsync(userId, cancellationToken);
        if (sessions.Count <= _options.Session.MaxConcurrentSessions)
        {
            return;
        }

        // ????????? ?????????? ?????
        var sessionsToRemove = sessions
            .OrderBy(s => s.LastActivity)
            .Take(sessions.Count - _options.Session.MaxConcurrentSessions);

        foreach (var session in sessionsToRemove)
        {
            await InvalidateSessionAsync(session.Id.ToString(), cancellationToken);
        }
    }

    // Key generation helpers
    private string GetSessionKey(string sessionId) => $"{_options.Redis.KeyPrefix}session:{sessionId}";
    private string GetRefreshTokenKey(string sessionId) => $"{_options.Redis.KeyPrefix}refresh:{sessionId}";
    private string GetRevokedTokenKey(string jti) => $"{_options.Redis.KeyPrefix}revoked:{jti}";
    private string GetUserSessionPattern(Guid userId) => $"{_options.Redis.KeyPrefix}session:*";
}