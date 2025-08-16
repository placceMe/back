namespace UsersService.DTOs
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? DeviceId { get; set; }
    }
}