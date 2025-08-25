using Microsoft.AspNetCore.Mvc;
using NotificationsService.Contracts;
using NotificationsService.Email;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Додайте Serilog для кращого логування
builder.Host.UseSerilog((context, configuration) =>
    configuration.WriteTo.Console()
                 .WriteTo.File("logs/notifications-.txt", rollingInterval: RollingInterval.Day));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Вибір провайдера по конфігу
var provider = builder.Configuration["Email:Provider"] ?? "Smtp";
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Email:Smtp"));

if (provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddSingleton<IEmailSender, SendGridEmailSender>();
else
    builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/email/send", async ([FromBody] SendEmailRequest req, IEmailSender sender, ILogger<Program> logger, CancellationToken ct) =>
{
    try
    {
        logger.LogInformation("Sending email to {To} with subject: {Subject}", req.To, req.Subject);
        await sender.SendAsync(req.To, req.Subject, req.HtmlBody, req.TextBody, ct);
        logger.LogInformation("Email sent successfully to {To}", req.To);
        return Results.Accepted();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to send email to {To}", req.To);
        return Results.Problem($"Failed to send email: {ex.Message}");
    }
});

app.MapPost("/api/email/password-reset", async ([FromBody] PasswordResetMessage msg, IEmailSender sender, ILogger<Program> logger, CancellationToken ct) =>
{
    try
    {
        logger.LogInformation("Sending password reset email to {To}", msg.To);
        var (subject, html, text) = EmailTemplates.PasswordReset(msg.UserDisplayName, msg.ResetUrl);
        await sender.SendAsync(msg.To, subject, html, text, ct);
        return Results.Accepted();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to send password reset email to {To}", msg.To);
        return Results.Problem($"Failed to send password reset email: {ex.Message}");
    }
});

app.MapPost("/api/email/confirm-registration", async ([FromBody] ConfirmRegistrationMessage msg, IEmailSender sender, ILogger<Program> logger, CancellationToken ct) =>
{
    try
    {
        logger.LogInformation("Sending registration confirmation email to {To}", msg.To);
        var (subject, html, text) = EmailTemplates.ConfirmRegistration(msg.UserDisplayName, msg.ConfirmationUrl);
        await sender.SendAsync(msg.To, subject, html, text, ct);
        return Results.Accepted();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to send registration confirmation email to {To}", msg.To);
        return Results.Problem($"Failed to send registration confirmation email: {ex.Message}");
    }
});

app.Run();