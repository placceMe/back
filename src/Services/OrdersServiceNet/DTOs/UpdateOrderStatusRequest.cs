using OrdersServiceNet.Models;

namespace OrdersServiceNet.DTOs;

public class UpdateOrderStatusRequest
{
    public required string Status { get; set; }
}