namespace NotificationsService.Contracts;

public class SendEmailRequest
{
    public required string To { get; set; }
    public required string Subject { get; set; }
    public required string HtmlBody { get; set; }
    public string? TextBody { get; set; }
}

public class PasswordResetMessage
{
    public required string To { get; set; }
    public required string UserDisplayName { get; set; }
    public required string ResetUrl { get; set; }
}

public class ConfirmRegistrationMessage
{
    public required string To { get; set; }
    public required string UserDisplayName { get; set; }
    public required string ConfirmationUrl { get; set; }
}

public class OrderCreatedMessage
{
    public required string To { get; set; }
    public required string UserDisplayName { get; set; }
    public required string OrderId { get; set; }
    public required decimal TotalAmount { get; set; }
    public required List<OrderItemInfo> Items { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Notes { get; set; }
}

public class OrderItemInfo
{
    public required string ProductName { get; set; }
    public required int Quantity { get; set; }
    public required decimal Price { get; set; }
}