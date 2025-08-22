using System.Text;
using System.Text.Json;

namespace UsersService.Services;

public interface INotificationServiceClient
{
    Task SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string to, string userDisplayName, string resetUrl, CancellationToken cancellationToken = default);
    Task SendRegistrationNotificationAsync(string to, string userDisplayName, string activationUrl, CancellationToken cancellationToken = default);
}

public class NotificationServiceClient : INotificationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationServiceClient> _logger;

    public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new SendEmailRequest
            {
                To = to,
                Subject = subject,
                HtmlBody = htmlBody,
                TextBody = textBody
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/email/send", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to send email. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
            }
            else
            {
                _logger.LogInformation("Email sent successfully to {To}", to);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", to);
            throw;
        }
    }
    public async Task SendRegistrationNotificationAsync(string to, string userDisplayName, string activationUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                To = to,
                UserDisplayName = userDisplayName,
                ActivationUrl = activationUrl
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/email/confirm-registration", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to send registration email. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
            }
            else
            {
                _logger.LogInformation("Registration email sent successfully to {To}", to);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending registration email to {To}", to);
            throw;
        }
    }

    public async Task SendPasswordResetEmailAsync(string to, string userDisplayName, string resetUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PasswordResetMessage
            {
                To = to,
                UserDisplayName = userDisplayName,
                ResetUrl = resetUrl
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/email/password-reset", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to send password reset email. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
            }
            else
            {
                _logger.LogInformation("Password reset email sent successfully to {To}", to);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {To}", to);
            throw;
        }
    }

    public async Task SendConfirmRegistrationEmailAsync(string to, string userDisplayName, string confirmationUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ConfirmRegistrationMessage
            {
                To = to,
                UserDisplayName = userDisplayName,
                ConfirmationUrl = confirmationUrl
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/email/confirm-registration", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to send confirmation registration email. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
            }
            else
            {
                _logger.LogInformation("Confirmation registration email sent successfully to {To}", to);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending confirmation registration email to {To}", to);
            throw;
        }
    }
}

internal class ConfirmRegistrationMessage
{
    public string To { get; set; }
    public string UserDisplayName { get; set; }
    public string ConfirmationUrl { get; set; }
}

internal class PasswordResetMessage
{
    public string To { get; set; }
    public string UserDisplayName { get; set; }
    public string ResetUrl { get; set; }
}

internal class SendEmailRequest
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string HtmlBody { get; set; }
    public string TextBody { get; set; }
}