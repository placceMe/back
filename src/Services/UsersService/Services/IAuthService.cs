using UsersService.DTOs;
using UsersService.Models;

namespace UsersService.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LogoutAsync(string jti, DateTimeOffset accessTokenExpiry);
    Task<AuthResponse> RefreshTokenAsync(Guid userId, string deviceId, string oldRefreshToken);
    Task<User?> GetCurrentUserAsync(string token);
    string GenerateJwtToken(User user);
    bool ValidateToken(string token);
}