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

    public static (string subject, string html, string text) OrderCreated(
        string userName,
        string orderId,
        decimal totalAmount,
        List<(string productName, int quantity, decimal price)> items,
        string? deliveryAddress = null,
        string? notes = null)
    {
        var subject = $"Замовлення #{orderId.Substring(0, 8)} успішно створено";

        var itemsHtml = string.Join("", items.Select(item => $@"
            <tr style=""border-bottom: 1px solid #eee;"">
                <td style=""padding: 10px; text-align: left;"">{HttpUtility.HtmlEncode(item.productName)}</td>
                <td style=""padding: 10px; text-align: center;"">{item.quantity}</td>
                <td style=""padding: 10px; text-align: right;"">{item.price:C}</td>
                <td style=""padding: 10px; text-align: right;"">{(item.price * item.quantity):C}</td>
            </tr>"));

        var html = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #ddd; border-radius: 8px; overflow: hidden;"">
                <div style=""background-color: #007bff; color: white; padding: 20px; text-align: center;"">
                    <h1 style=""margin: 0; font-size: 24px;"">Замовлення прийнято! 🎉</h1>
                </div>
                
                <div style=""padding: 20px;"">
                    <p style=""font-size: 16px; margin-bottom: 20px;"">Вітаємо, <strong>{HttpUtility.HtmlEncode(userName)}</strong>!</p>
                    
                    <p>Ваше замовлення <strong>#{orderId.Substring(0, 8)}</strong> було успішно створено та передано в обробку.</p>
                    
                    <div style=""background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                        <h3 style=""margin-top: 0; color: #333;"">Деталі замовлення:</h3>
                        
                        <table style=""width: 100%; border-collapse: collapse; margin-bottom: 15px;"">
                            <thead>
                                <tr style=""background-color: #e9ecef;"">
                                    <th style=""padding: 10px; text-align: left; border-bottom: 2px solid #dee2e6;"">Товар</th>
                                    <th style=""padding: 10px; text-align: center; border-bottom: 2px solid #dee2e6;"">Кількість</th>
                                    <th style=""padding: 10px; text-align: right; border-bottom: 2px solid #dee2e6;"">Ціна</th>
                                    <th style=""padding: 10px; text-align: right; border-bottom: 2px solid #dee2e6;"">Сума</th>
                                </tr>
                            </thead>
                            <tbody>
                                {itemsHtml}
                            </tbody>
                            <tfoot>
                                <tr style=""background-color: #e9ecef; font-weight: bold;"">
                                    <td colspan=""3"" style=""padding: 15px; text-align: right; border-top: 2px solid #dee2e6;"">Загальна сума:</td>
                                    <td style=""padding: 15px; text-align: right; border-top: 2px solid #dee2e6; color: #007bff; font-size: 18px;"">{totalAmount:C}</td>
                                </tr>
                            </tfoot>
                        </table>
                        
                        {(!string.IsNullOrEmpty(deliveryAddress) ? $@"
                        <p><strong>Адреса доставки:</strong><br>
                        {HttpUtility.HtmlEncode(deliveryAddress)}</p>" : "")}
                        
                        {(!string.IsNullOrEmpty(notes) ? $@"
                        <p><strong>Примітки:</strong><br>
                        {HttpUtility.HtmlEncode(notes)}</p>" : "")}
                    </div>
                    
                    <div style=""background-color: #d4edda; border: 1px solid #c3e6cb; color: #155724; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                        <h4 style=""margin-top: 0;"">Що далі?</h4>
                        <ul style=""margin-bottom: 0; padding-left: 20px;"">
                            <li>Ваше замовлення буде оброблено протягом 1-2 робочих днів</li>
                            <li>Ми зв'яжемось з вами для підтвердження деталей</li>
                            <li>Після підтвердження замовлення буде передано в доставку</li>
                        </ul>
                    </div>
                    
                    <p style=""color: #666; font-size: 14px; margin-top: 30px;"">
                        Дякуємо, що обрали наш маркетплейс!<br>
                        З повагою, команда Marketplace
                    </p>
                </div>
            </div>";

        var itemsText = string.Join("\n", items.Select(item => $"  - {item.productName} x{item.quantity} = {(item.price * item.quantity):C}"));
        var text = $@"
Замовлення #{orderId.Substring(0, 8)} успішно створено!

Вітаємо, {userName}!

Ваше замовлення було успішно створено та передано в обробку.

Деталі замовлення:
{itemsText}

Загальна сума: {totalAmount:C}

{(!string.IsNullOrEmpty(deliveryAddress) ? $"Адреса доставки: {deliveryAddress}\n" : "")}
{(!string.IsNullOrEmpty(notes) ? $"Примітки: {notes}\n" : "")}

Що далі?
- Ваше замовлення буде оброблено протягом 1-2 робочих днів
- Ми зв'яжемось з вами для підтвердження деталей  
- Після підтвердження замовлення буде передано в доставку

Дякуємо, що обрали наш маркетплейс!
З повагою, команда Marketplace";

        return (subject, html, text);
    }
}
