using System.Web;

namespace NotificationsService.Email;

public static class EmailTemplates
{
    public static (string subject, string html, string text) PasswordReset(string userName, string resetUrl)
    {
        var subject = "Password Reset Request";
        var html = $@"
            <h2>Password Reset</h2>
            <p>Hello {userName},</p>
            <p>Click <a href=""{resetUrl}"">here</a> to reset your password.</p>
            <p>If you didn't request this, please ignore this email.</p>";
        var text = $"Hello {userName}, visit {resetUrl} to reset your password.";

        return (subject, html, text);
    }

    public static (string subject, string html, string text) ConfirmRegistration(string userName, string confirmUrl)
    {
        var subject = "Confirm Your Registration";
        var html = $@"
            <h2>Welcome to Marketplace!</h2>
            <p>Hello {userName},</p>
            <p>Click <a href=""{confirmUrl}"">here</a> to confirm your registration.</p>";
        var text = $"Hello {userName}, visit {confirmUrl} to confirm your registration.";

        return (subject, html, text);
    }
}
