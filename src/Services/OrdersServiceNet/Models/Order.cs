namespace OrdersServiceNet.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = OrderStatus.Pending;
    public string? Notes { get; set; }
    public string? DeliveryAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public static class OrderStatus
{
    public static readonly string Pending = "Pending";
    public static readonly string Confirmed = "Confirmed";
    public static readonly string Shipped = "Shipped";
    public static readonly string Delivered = "Delivered";
    public static readonly string Cancelled = "Cancelled";
    public static readonly string Rejected = "Rejected";
}