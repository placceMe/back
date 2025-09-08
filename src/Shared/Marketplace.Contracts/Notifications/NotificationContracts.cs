using System.ComponentModel.DataAnnotations;

namespace Marketplace.Contracts.Notifications;

/// <summary>
/// Send email request
/// </summary>
public class SendEmailRequest
{
    [Required]
    [EmailAddress]
    public string To { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string HtmlBody { get; set; } = string.Empty;

    public string? TextBody { get; set; }
}

/// <summary>
/// Password reset message
/// </summary>
public class PasswordResetMessage
{
    [Required]
    [EmailAddress]
    public string To { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string UserDisplayName { get; set; } = string.Empty;

    [Required]
    [Url]
    public string ResetUrl { get; set; } = string.Empty;
}

/// <summary>
/// Confirm registration message
/// </summary>
public class ConfirmRegistrationMessage
{
    [Required]
    [EmailAddress]
    public string To { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string UserDisplayName { get; set; } = string.Empty;

    [Required]
    [Url]
    public string ConfirmationUrl { get; set; } = string.Empty;
}

/// <summary>
/// Order created message
/// </summary>
public class OrderCreatedMessage
{
    [Required]
    [EmailAddress]
    public string To { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string UserDisplayName { get; set; } = string.Empty;

    [Required]
    public string OrderId { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [Required]
    public List<OrderItemInfo> Items { get; set; } = new();

    [StringLength(500)]
    public string? DeliveryAddress { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// Order item information
/// </summary>
public class OrderItemInfo
{
    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
}
