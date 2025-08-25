using SendGrid;
using SendGrid.Helpers.Mail;

namespace NotificationsService.Email;

public class SendGridEmailSender : IEmailSender
{
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(ILogger<SendGridEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string htmlBody, string textBody, CancellationToken ct = default)
    {
        _logger.LogInformation("SendGrid email would be sent to {To} with subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
