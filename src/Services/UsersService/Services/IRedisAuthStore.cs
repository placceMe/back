namespace UsersService.Services;

public interface IRedisAuthStore
{
    Task StoreRefreshTokenAsync(Guid userId, string deviceId, string refreshToken, DateTime expiry, CancellationToken cancellationToken = default);
    Task<bool> ValidateAndRotateRefreshTokenAsync(Guid userId, string deviceId, string oldRefreshToken, string newRefreshToken, DateTime newExpiry, CancellationToken cancellationToken = default);
    Task RevokeJtiAsync(string jti, DateTimeOffset expiry, CancellationToken cancellationToken = default);
    Task<bool> IsJtiRevokedAsync(string jti, CancellationToken cancellationToken = default);
    Task StorePasswordResetTokenAsync(string token, Guid userId, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task<Guid?> ValidatePasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokePasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}