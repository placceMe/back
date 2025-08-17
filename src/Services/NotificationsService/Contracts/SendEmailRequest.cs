namespace NotificationsService.Contracts;

public record SendEmailRequest(
    string To,
    string Subject,
    string HtmlBody,
    string? TextBody = null
);
