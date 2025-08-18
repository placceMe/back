using System.ComponentModel.DataAnnotations;

namespace UsersService.DTOs;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Токен є обов'язковим")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль є обов'язковим")]
    [MinLength(6, ErrorMessage = "Пароль має містити принаймні 6 символів")]
    public string NewPassword { get; set; } = string.Empty;
}