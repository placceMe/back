using Microsoft.AspNetCore.Mvc;
using NotificationsService.Contracts;
using NotificationsService.Email;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Вибір провайдера по конфігу
var provider = builder.Configuration["Email:Provider"] ?? "Smtp";
if (provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddSingleton<IEmailSender, SendGridEmailSender>();
else
    builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/email/send", async ([FromBody] SendEmailRequest req, IEmailSender sender, CancellationToken ct) =>
{
    await sender.SendAsync(req.To, req.Subject, req.HtmlBody, req.TextBody, ct);
    return Results.Accepted();
});

app.MapPost("/api/email/password-reset", async ([FromBody] PasswordResetMessage msg, IEmailSender sender, CancellationToken ct) =>
{
    var (subject, html, text) = EmailTemplates.PasswordReset(msg.UserDisplayName, msg.ResetUrl);
    await sender.SendAsync(msg.To, subject, html, text, ct);
    return Results.Accepted();
});

app.MapPost("/api/email/confirm-registration", async ([FromBody] ConfirmRegistrationMessage msg, IEmailSender sender, CancellationToken ct) =>
{
    var (subject, html, text) = EmailTemplates.ConfirmRegistration(msg.UserDisplayName, msg.ConfirmationUrl);
    await sender.SendAsync(msg.To, subject, html, text, ct);
    return Results.Accepted();
});

app.Run();
