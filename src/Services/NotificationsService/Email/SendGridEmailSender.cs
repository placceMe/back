using SendGrid;
using SendGrid.Helpers.Mail;

namespace NotificationsService.Email;

public class SendGridEmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;
    private readonly string _apiKey;
    private readonly string _fromName;
    private readonly string _fromAddress;

    public SendGridEmailSender(IConfiguration cfg)
    {
        _cfg = cfg;
        _apiKey = _cfg["Email:SendGrid:ApiKey"]!;
        _fromName = _cfg["Email:FromName"]!;
        _fromAddress = _cfg["Email:FromAddress"]!;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_fromAddress, _fromName);
        var toAddr = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toAddr, subject, textBody ?? string.Empty, htmlBody);
        var resp = await client.SendEmailAsync(msg, ct);

        if ((int)resp.StatusCode >= 400)
        {
            var body = await resp.Body.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"SendGrid failed: {(int)resp.StatusCode} {body}");
        }
    }
}
