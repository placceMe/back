namespace NotificationsService.Contracts;

public record PasswordResetMessage(
    string To,
    string ResetUrl,
    string? UserDisplayName
);
