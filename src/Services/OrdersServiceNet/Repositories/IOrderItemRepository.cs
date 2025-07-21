using OrdersServiceNet.Models;

namespace OrdersServiceNet.Repositories;

public interface IOrderItemRepository
{
    Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(Guid orderId);
    Task<OrderItem> CreateOrderItemAsync(OrderItem orderItem);
    Task<bool> DeleteOrderItemsByOrderIdAsync(Guid orderId);
}