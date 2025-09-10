using Microsoft.EntityFrameworkCore;
using OrdersServiceNet.Data;
using OrdersServiceNet.Models;

namespace OrdersServiceNet.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersBySellerIdAsync(Guid sellerId, Dictionary<Guid, Guid> productSellerMap)
    {
        // Get all product IDs that belong to this seller
        var sellerProductIds = productSellerMap
            .Where(kvp => kvp.Value == sellerId)
            .Select(kvp => kvp.Key)
            .ToHashSet();

        if (!sellerProductIds.Any())
            return Enumerable.Empty<Order>();

        // Get orders that contain products from this seller
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.OrderItems.Any(oi => sellerProductIds.Contains(oi.ProductId)))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order> UpdateOrderAsync(Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteOrderAsync(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }
}