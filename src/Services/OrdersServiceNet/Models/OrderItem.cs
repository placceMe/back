namespace OrdersServiceNet.Models;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
}