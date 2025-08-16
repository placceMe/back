namespace UsersService.DTOs;

public class RefreshTokenRequest
{
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}