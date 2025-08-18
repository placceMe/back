using System.ComponentModel.DataAnnotations;

namespace UsersService.DTOs;

public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "Email є обов'язковим")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    public string Email { get; set; } = string.Empty;
}