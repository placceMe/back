using OrdersServiceNet.Models;

namespace OrdersServiceNet.DTOs;

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}