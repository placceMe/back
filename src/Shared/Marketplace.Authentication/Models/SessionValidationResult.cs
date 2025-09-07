namespace Marketplace.Authentication.Models;

public class SessionValidationResult
{
    public bool IsValid { get; set; }
    public AuthSession? Session { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsExpired { get; set; }
    public bool RequiresRefresh { get; set; }

    public static SessionValidationResult Success(AuthSession session) => new()
    {
        IsValid = true,
        Session = session
    };

    public static SessionValidationResult Failed(string errorMessage) => new()
    {
        IsValid = false,
        ErrorMessage = errorMessage
    };

    public static SessionValidationResult Expired() => new()
    {
        IsValid = false,
        IsExpired = true,
        ErrorMessage = "Session has expired"
    };

    public static SessionValidationResult RefreshRequired(AuthSession session) => new()
    {
        IsValid = true,
        Session = session,
        RequiresRefresh = true
    };
}