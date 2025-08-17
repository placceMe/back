using System.Web;

namespace NotificationsService.Email;

public static class EmailTemplates
{
    public static (string subject, string html, string text) PasswordReset(string? displayName, string resetUrl)
    {
        var name = string.IsNullOrWhiteSpace(displayName) ? "there" : displayName;
        var subject = "Відновлення паролю";
        var html =
$@"<div style=""font-family:Arial,sans-serif"">
  <h2>Відновлення паролю</h2>
  <p>Привіт, {HttpUtility.HtmlEncode(name)}!</p>
  <p>Щоб скинути пароль, натисни кнопку нижче:</p>
  <p><a href=""{resetUrl}"" style=""background:#2563eb;color:#fff;padding:10px 16px;border-radius:8px;text-decoration:none"">Скинути пароль</a></p>
  <p>Якщо ти не запитував(ла) зміну паролю — просто проігноруй цей лист.</p>
</div>";
        var text =
$@"Відновлення паролю

Привіт, {name}!
Посилання для скидання паролю: {resetUrl}

Якщо ти не запитував(ла) зміну паролю — проігноруй цей лист.";
        return (subject, html, text);
    }
}
