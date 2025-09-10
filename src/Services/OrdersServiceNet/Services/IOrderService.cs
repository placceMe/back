using Marketplace.Contracts.Orders;
using Marketplace.Contracts.Common;

namespace OrdersServiceNet.Services;

public interface IOrderService
{
    Task<CreateOrdersResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderResponse?> GetOrderByIdAsync(Guid id);
    Task<IEnumerable<OrderResponse>> GetOrdersByUserIdAsync(Guid userId);
    Task<IEnumerable<OrderResponse>> GetOrdersBySellerIdAsync(Guid sellerId);
    Task<IEnumerable<OrderResponse>> GetAllOrdersAsync();
    Task<OrderResponse?> ConfirmOrderAsync(Guid id);
    Task<OrderResponse?> RejectOrderAsync(Guid id);
    Task<OrderResponse?> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request);
    Task<bool> DeleteOrderAsync(Guid id);
}
