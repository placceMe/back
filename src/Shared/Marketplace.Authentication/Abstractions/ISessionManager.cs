using Marketplace.Authentication.Models;

namespace Marketplace.Authentication.Abstractions;

public interface ISessionManager
{
    // Session Management
    Task<SessionToken> CreateSessionAsync(CreateSessionRequest request, CancellationToken cancellationToken = default);
    Task<SessionValidationResult> ValidateSessionAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task<SessionValidationResult> ValidateSessionByIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<bool> RefreshSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<bool> InvalidateSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<bool> InvalidateAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<AuthSession>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Token Operations
    Task<SessionToken?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> IsTokenRevokedAsync(string tokenId, CancellationToken cancellationToken = default);
    
    // Utility Methods
    Task<bool> UpdateSessionActivityAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<bool> UpdateSessionMetadataAsync(string sessionId, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
    Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default);
}