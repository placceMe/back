namespace UsersService.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string State { get; set; } = UserState.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public List<string> Roles { get; set; } = new List<string> { };
}

public class RegistrationUser : User
{
    public string ActivationCode { get; set; } = null!;
    public DateTime ActivationCodeExpiresAt { get; set; }

}
