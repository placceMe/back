namespace NotificationsService.Contracts;

public record PasswordResetMessage(string To, string UserDisplayName, string ResetUrl);
