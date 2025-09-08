using UsersService.Models;
using Marketplace.Contracts.Users;

namespace UsersService.Services;

public interface IUserService
{
    // User management methods
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> SoftDeleteAsync(Guid id);
    Task<bool> MakeSellerAsync(Guid id);
    Task<bool> UpdateRolesAsync(Guid userId, List<string> roles);
    Task<bool> ChangeStateAsync(Guid userId, string newState);
    Task<bool> ConfirmUserAsync(Guid token);

    // Authentication methods
    Task<AuthResponse> LoginAsync(LoginDto request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LogoutAsync(string jti, DateTimeOffset accessTokenExpiry);
    Task<AuthResponse> RefreshTokenAsync(Guid userId, string deviceId, string oldRefreshToken);
    Task<User?> GetCurrentUserAsync(string token);
    string GenerateJwtToken(User user);
    bool ValidateToken(string token);
    Task<AuthResponse> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<AuthResponse> ForgotPasswordAsync(string email);
    Task<AuthResponse> ResetPasswordAsync(string token, string newPassword);
    Task<UserInfoWithSellerInfo?> GetWithSellerInfoAsync(Guid id);
    Task<(IEnumerable<UserInfoWithSellerInfo> Users, int TotalCount)> GetAllWithSellerInfoAsync(int page = 1, int pageSize = 10);
}
