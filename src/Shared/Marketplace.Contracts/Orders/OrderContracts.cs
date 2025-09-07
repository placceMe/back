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