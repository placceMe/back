namespace Marketplace.Authentication.Models;

public class CreateSessionRequest
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string DeviceId { get; set; } = "default";
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public TimeSpan SessionDuration { get; set; } = TimeSpan.FromHours(24);
    public Dictionary<string, object>? Metadata { get; set; }
}