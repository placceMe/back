namespace NotificationsService.Email;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, string textBody, CancellationToken ct = default);
}
