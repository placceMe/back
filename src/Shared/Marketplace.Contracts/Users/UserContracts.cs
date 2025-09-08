using System.ComponentModel.DataAnnotations;

namespace Marketplace.Contracts.Users;

/// <summary>
/// User data transfer object
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Url]
    public string AvatarUrl { get; set; } = string.Empty;

    public bool IsEmailConfirmed { get; set; }
    public bool IsPhoneConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }

    public List<string> Roles { get; set; } = new();
    public SellerInfoDto? SellerInfo { get; set; }
}

/// <summary>
/// Create user request
/// </summary>
public class CreateUserDto
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Update user request
/// </summary>
public class UpdateUserDto
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Url]
    public string AvatarUrl { get; set; } = string.Empty;
}

/// <summary>
/// Seller information DTO
/// </summary>
public class SellerInfoDto
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string Schedule { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public List<ContactDto> Contacts { get; set; } = new();
}

/// <summary>
/// Create seller info request
/// </summary>
public class CreateSellerInfoDto
{
    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string Schedule { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    public List<ContactDto> Contacts { get; set; } = new();
}

/// <summary>
/// Update seller info request
/// </summary>
public class UpdateSellerInfoDto
{
    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string Schedule { get; set; } = string.Empty;

    public List<ContactDto> Contacts { get; set; } = new();
}

/// <summary>
/// Seller info response
/// </summary>
public class SellerInfoResponseDto
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string Schedule { get; set; } = string.Empty;

    public List<ContactDto> Contacts { get; set; } = new();

    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Contact information DTO
/// </summary>
public class ContactDto
{
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty; // "phone", "email", "telegram", etc.

    [Required]
    [StringLength(200)]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Login request
/// </summary>
public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Login response
/// </summary>
public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}

/// <summary>
/// Authentication response
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserInfo? User { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? DeviceId { get; set; }
    public string? Token { get; set; }
}

/// <summary>
/// User information (simplified)
/// </summary>
public class UserInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// User information with seller info
/// </summary>
public class UserInfoWithSellerInfo : UserInfo
{
    public SellerInfoDto? SellerInfo { get; set; } = null;
}

/// <summary>
/// Register request
/// </summary>
public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }
}

/// <summary>
/// Change password request
/// </summary>
public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Confirm registration request
/// </summary>
public class ConfirmRegistrationRequest
{
    [Required(ErrorMessage = "Токен підтвердження обов'язковий")]
    public Guid Token { get; set; } = Guid.Empty;

    public Guid UserId { get; set; } = Guid.Empty;
}

/// <summary>
/// Forgot password request
/// </summary>
public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "Email є обов'язковим")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Reset password request
/// </summary>
public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Токен є обов'язковим")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль є обов'язковим")]
    [MinLength(6, ErrorMessage = "Пароль має містити принаймні 6 символів")]
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Refresh token request
/// </summary>
public class RefreshTokenRequest
{
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Get seller info by IDs request
/// </summary>
public class GetSellerInfoByIdsRequest
{
    [Required]
    [MinLength(1)]
    public List<Guid> Ids { get; set; } = new();
}

/// <summary>
/// Paginated users with seller info response
/// </summary>
public class PaginatedUsersWithSellerInfoDto
{
    public List<UserInfoWithSellerInfo> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}