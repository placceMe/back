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