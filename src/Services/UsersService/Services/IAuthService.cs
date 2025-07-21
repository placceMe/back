using UsersService.DTOs;
using UsersService.Models;

namespace UsersService.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LogoutAsync();
    Task<User?> GetCurrentUserAsync(string token);
    string GenerateJwtToken(User user);
    bool ValidateToken(string token);
}