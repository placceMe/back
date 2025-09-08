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

// Configure JSON options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

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
        await sender.SendAsync(req.To, req.Subject, req.HtmlBody, req.TextBody ?? string.Empty, ct);
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

app.MapPost("/api/email/order-created", async (HttpContext context, IEmailSender sender, ILogger<Program> logger, CancellationToken ct) =>
{
    try
    {
        // Read request body with UTF-8 encoding
        using var reader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        logger.LogInformation("Received raw order confirmation email request: {Body}", body);

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var msg = System.Text.Json.JsonSerializer.Deserialize<OrderCreatedMessage>(body, options);

        if (msg == null)
        {
            logger.LogError("Failed to deserialize order confirmation email request");
            return Results.BadRequest("Invalid request format");
        }

        logger.LogInformation("Successfully deserialized. Sending order confirmation email to {To} for order {OrderId}", msg.To, msg.OrderId);
        var items = msg.Items.Select(i => (i.ProductName, i.Quantity, i.Price)).ToList();
        var (subject, html, text) = EmailTemplates.OrderCreated(
            msg.UserDisplayName,
            msg.OrderId,
            msg.TotalAmount,
            items,
            msg.DeliveryAddress,
            msg.Notes);
        await sender.SendAsync(msg.To, subject, html, text, ct);
        logger.LogInformation("Order confirmation email sent successfully to {To} for order {OrderId}", msg.To, msg.OrderId);
        return Results.Accepted();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to send order confirmation email: {Message}", ex.Message);
        return Results.Problem($"Failed to send order confirmation email: {ex.Message}");
    }
});

app.Run();