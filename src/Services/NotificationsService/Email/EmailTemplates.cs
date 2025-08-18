using System.Web;

namespace NotificationsService.Email;

public static class EmailTemplates
{
  public static (string subject, string html, string text) PasswordReset(string userDisplayName, string resetUrl)
  {
    var subject = "Скидання паролю - Marketplace";

    var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Скидання паролю</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #2c3e50;'>Привіт, {userDisplayName}!</h2>
        
        <p>Ви запросили скидання паролю для вашого акаунту в Marketplace.</p>
        
        <p>Натисніть кнопку нижче, щоб створити новий пароль:</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{resetUrl}' 
               style='background-color: #3498db; 
                      color: white; 
                      padding: 12px 30px; 
                      text-decoration: none; 
                      border-radius: 5px; 
                      display: inline-block;
                      font-weight: bold;'>
                Скинути пароль
            </a>
        </div>
        
        <p style='color: #7f8c8d; font-size: 14px;'>
            Це посилання дійсне протягом 1 години з моменту відправки.
        </p>
        
        <p style='color: #7f8c8d; font-size: 14px;'>
            Якщо ви не запрошували скидання паролю, просто ігноруйте цей лист.
        </p>
        
        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
        
        <p style='color: #95a5a6; font-size: 12px;'>
            Якщо кнопка не працює, скопіюйте та вставте це посилання у ваш браузер:<br>
            <a href='{resetUrl}' style='color: #3498db; word-break: break-all;'>{resetUrl}</a>
        </p>
    </div>
</body>
</html>";

    var text = $@"Привіт, {userDisplayName}!

Ви запросили скидання паролю для вашого акаунту в Marketplace.

Перейдіть за посиланням для створення нового паролю:
{resetUrl}

Це посилання дійсне протягом 1 години з моменту відправки.

Якщо ви не запрошували скидання паролю, просто ігноруйте цей лист.";

    return (subject, html, text);
  }
}
