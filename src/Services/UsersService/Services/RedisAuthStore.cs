using StackExchange.Redis;

namespace UsersService.Services;

public class RedisAuthStore : IRedisAuthStore
{
    private readonly IDatabase _database;

    public RedisAuthStore(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task StoreRefreshTokenAsync(Guid userId, string deviceId, string refreshToken, DateTime expiry, CancellationToken cancellationToken = default)
    {
        var key = $"refresh_token:{userId}:{deviceId}";
        var timeToLive = expiry - DateTime.UtcNow;
        await _database.StringSetAsync(key, refreshToken, timeToLive);
    }

    public async Task<bool> ValidateAndRotateRefreshTokenAsync(Guid userId, string deviceId, string oldRefreshToken, string newRefreshToken, DateTime newExpiry, CancellationToken cancellationToken = default)
    {
        var key = $"refresh_token:{userId}:{deviceId}";
        var storedToken = await _database.StringGetAsync(key);

        if (!storedToken.HasValue || storedToken != oldRefreshToken)
            return false;

        // Store new token
        var timeToLive = newExpiry - DateTime.UtcNow;
        await _database.StringSetAsync(key, newRefreshToken, timeToLive);

        return true;
    }

    public async Task RevokeJtiAsync(string jti, DateTimeOffset expiry, CancellationToken cancellationToken = default)
    {
        var key = $"revoked_jti:{jti}";
        var timeToLive = expiry - DateTimeOffset.UtcNow;
        await _database.StringSetAsync(key, "revoked", timeToLive.Duration());
    }

    public async Task<bool> IsJtiRevokedAsync(string jti, CancellationToken cancellationToken = default)
    {
        var key = $"revoked_jti:{jti}";
        return await _database.KeyExistsAsync(key);
    }

    public async Task StorePasswordResetTokenAsync(string token, Guid userId, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var key = $"password_reset_token:{token}";
        await _database.StringSetAsync(key, userId.ToString(), expiry);
    }

    public async Task<Guid?> ValidatePasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var key = $"password_reset_token:{token}";
        var userIdString = await _database.StringGetAsync(key);

        if (!userIdString.HasValue)
            return null;

        if (Guid.TryParse(userIdString, out var userId))
            return userId;

        return null;
    }

    public async Task RevokePasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var key = $"password_reset_token:{token}";
        await _database.KeyDeleteAsync(key);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Відкликаємо всі refresh токени користувача
        var refreshTokenPattern = $"refresh_token:{userId}:*";
        var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
        var refreshTokenKeys = server.Keys(pattern: refreshTokenPattern);

        if (refreshTokenKeys.Any())
        {
            await _database.KeyDeleteAsync(refreshTokenKeys.ToArray());
        }
    }
}