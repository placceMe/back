using OrdersServiceNet.DTOs;

namespace OrdersServiceNet.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderResponse?> GetOrderByIdAsync(Guid id);
    Task<IEnumerable<OrderResponse>> GetOrdersByUserIdAsync(Guid userId);
    Task<IEnumerable<OrderResponse>> GetAllOrdersAsync();
    Task<OrderResponse?> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request);
    Task<bool> DeleteOrderAsync(Guid id);
}