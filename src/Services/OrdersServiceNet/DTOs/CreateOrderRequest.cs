namespace OrdersServiceNet.DTOs;

public class CreateOrderRequest
{
    public Guid UserId { get; set; }
    public string? Notes { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}