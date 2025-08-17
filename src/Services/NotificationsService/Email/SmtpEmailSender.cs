using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace NotificationsService.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;
    private readonly string _fromName;
    private readonly string _fromAddress;
    private readonly string _host;
    private readonly int _port;
    private readonly string? _user;
    private readonly string? _password;
    private readonly bool _useSsl;

    public SmtpEmailSender(IConfiguration cfg)
    {
        _cfg = cfg;
        _fromName = _cfg["Email:FromName"]!;
        _fromAddress = _cfg["Email:FromAddress"]!;
        _host = _cfg["Email:Smtp:Host"]!;
        _port = int.Parse(_cfg["Email:Smtp:Port"]!);
        _user = _cfg["Email:Smtp:User"];
        _password = _cfg["Email:Smtp:Password"];
        _useSsl = bool.Parse(_cfg["Email:Smtp:UseSsl"] ?? "false");
    }

    public async Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_fromName, _fromAddress));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;

        var body = new BodyBuilder { HtmlBody = htmlBody, TextBody = textBody };
        msg.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        var secure = _useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
        await client.ConnectAsync(_host, _port, secure, ct);

        if (!string.IsNullOrEmpty(_user))
            await client.AuthenticateAsync(_user, _password, ct);

        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }
}
