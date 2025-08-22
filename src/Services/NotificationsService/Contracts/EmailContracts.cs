namespace NotificationsService.Contracts;

public class SendEmailRequest
{
    public required string To { get; set; }
    public required string Subject { get; set; }
    public required string HtmlBody { get; set; }
    public string? TextBody { get; set; }
}

public class PasswordResetMessage
{
    public required string To { get; set; }
    public required string UserDisplayName { get; set; }
    public required string ResetUrl { get; set; }
}

public class ConfirmRegistrationMessage
{
    public required string To { get; set; }
    public required string UserDisplayName { get; set; }
    public required string ConfirmationUrl { get; set; }
}