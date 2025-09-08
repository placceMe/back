using System.ComponentModel.DataAnnotations;

namespace Marketplace.Contracts.Orders;

/// <summary>
/// Order data transfer object
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }

    [Required]
    public Guid BuyerId { get; set; }

    [Required]
    [Range(1, uint.MaxValue)]
    public uint TotalAmount { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [StringLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Order item DTO
/// </summary>
public class OrderItemDto
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid SellerId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Range(1, uint.MaxValue)]
    public uint PricePerItem { get; set; }

    [Required]
    [Range(1, uint.MaxValue)]
    public uint TotalPrice { get; set; }

    public string ProductTitle { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
}

/// <summary>
/// Create order request
/// </summary>
public class CreateOrderDto
{
    [Required]
    public Guid BuyerId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "Order must contain at least one item")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Create order item request
/// </summary>
public class CreateOrderItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

/// <summary>
/// Update order status request
/// </summary>
public class UpdateOrderStatusDto
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Create order request (from OrdersServiceNet)
/// </summary>
public class CreateOrderRequest
{
    [Required]
    public Guid UserId { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(500)]
    public string? DeliveryAddress { get; set; }

    [Required]
    [MinLength(1)]
    public List<OrderItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Order item request
/// </summary>
public class OrderItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

/// <summary>
/// Order response (from OrdersServiceNet)
/// </summary>
public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(500)]
    public string? DeliveryAddress { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<OrderItemResponse> Items { get; set; } = new();
}

/// <summary>
/// Order item response
/// </summary>
public class OrderItemResponse
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public ProductInfo? Product { get; set; }
}

/// <summary>
/// Product information (simplified)
/// </summary>
public class ProductInfo
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}

/// <summary>
/// Update order status request (from OrdersServiceNet)
/// </summary>
public class UpdateOrderStatusRequest
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// User information for orders
/// </summary>
public class OrderUserInfo
{
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    public string? DisplayName => !string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName)
        ? $"{FirstName} {LastName}".Trim()
        : Email.Split('@')[0];
}