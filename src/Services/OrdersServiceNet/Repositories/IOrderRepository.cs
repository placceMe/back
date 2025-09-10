using OrdersServiceNet.Models;

namespace OrdersServiceNet.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateOrderAsync(Order order);
    Task<Order?> GetOrderByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
    Task<IEnumerable<Order>> GetOrdersBySellerIdAsync(Guid sellerId, Dictionary<Guid, Guid> productSellerMap);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<Order> UpdateOrderAsync(Order order);
    Task<bool> DeleteOrderAsync(Guid id);
}