using System.ComponentModel.DataAnnotations;

namespace UsersService.DTOs;

public class ConfirmRegistrationRequest
{
    [Required(ErrorMessage = "Токен підтвердження обов'язковий")]
    public Guid Token { get; set; } = Guid.Empty;
    public Guid UserId { get; set; } = Guid.Empty;
}