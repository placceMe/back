using Microsoft.EntityFrameworkCore;
using OrdersServiceNet.Data;
using OrdersServiceNet.Models;

namespace OrdersServiceNet.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly OrdersDbContext _context;

    public OrderItemRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(Guid orderId)
    {
        return await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<OrderItem> CreateOrderItemAsync(OrderItem orderItem)
    {
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();
        return orderItem;
    }

    public async Task<bool> DeleteOrderItemsByOrderIdAsync(Guid orderId)
    {
        var orderItems = await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();

        if (!orderItems.Any()) return false;

        _context.OrderItems.RemoveRange(orderItems);
        await _context.SaveChangesAsync();
        return true;
    }
}