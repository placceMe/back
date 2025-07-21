using OrdersServiceNet.DTOs;
using OrdersServiceNet.Models;
using OrdersServiceNet.Repositories;

namespace OrdersServiceNet.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ProductsServiceClient _productsClient;
    private readonly UsersServiceClient _usersClient;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        ProductsServiceClient productsClient,
        UsersServiceClient usersClient,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productsClient = productsClient;
        _usersClient = usersClient;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        // Validate user exists
        var userExists = await _usersClient.UserExistsAsync(request.UserId);
        if (!userExists)
            throw new ArgumentException($"User {request.UserId} not found");

        // Get all unique product IDs
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();

        // Fetch all products in one request
        var productsDict = await _productsClient.GetProductsManyAsync(productIds);

        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        // Validate products and calculate total
        foreach (var item in request.Items)
        {
            if (!productsDict.TryGetValue(item.ProductId, out var product))
                throw new ArgumentException($"Product {item.ProductId} not found");

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = product.Price
            };

            orderItems.Add(orderItem);
            totalAmount += product.Price * item.Quantity;
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            Notes = request.Notes,
            DeliveryAddress = request.DeliveryAddress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = orderItems
        };

        var createdOrder = await _orderRepository.CreateOrderAsync(order);
        _logger.LogInformation("Order {OrderId} created for user {UserId}", createdOrder.Id, createdOrder.UserId);

        return await MapToOrderResponseAsync(createdOrder);
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
    {
        var order = await _orderRepository.GetOrderByIdAsync(id);
        return order != null ? await MapToOrderResponseAsync(order) : null;
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersByUserIdAsync(Guid userId)
    {
        var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
        var responses = new List<OrderResponse>();

        foreach (var order in orders)
        {
            responses.Add(await MapToOrderResponseAsync(order));
        }

        return responses;
    }

    public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllOrdersAsync();
        var responses = new List<OrderResponse>();

        foreach (var order in orders)
        {
            responses.Add(await MapToOrderResponseAsync(order));
        }

        return responses;
    }

    public async Task<OrderResponse?> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request)
    {
        var order = await _orderRepository.GetOrderByIdAsync(id);
        if (order == null) return null;

        order.Status = request.Status;
        var updatedOrder = await _orderRepository.UpdateOrderAsync(order);

        _logger.LogInformation("Order {OrderId} status updated to {Status}", id, request.Status);

        return await MapToOrderResponseAsync(updatedOrder);
    }

    public async Task<bool> DeleteOrderAsync(Guid id)
    {
        var result = await _orderRepository.DeleteOrderAsync(id);
        if (result)
        {
            _logger.LogInformation("Order {OrderId} deleted", id);
        }
        return result;
    }

    private async Task<OrderResponse> MapToOrderResponseAsync(Order order)
    {
        var response = new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            Notes = order.Notes,
            DeliveryAddress = order.DeliveryAddress,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = new List<OrderItemResponse>()
        };

        // Get all unique product IDs from order items
        var productIds = order.OrderItems.Select(i => i.ProductId).Distinct().ToList();

        // Fetch all products in one request
        var productsDict = await _productsClient.GetProductsManyAsync(productIds);

        foreach (var item in order.OrderItems)
        {
            productsDict.TryGetValue(item.ProductId, out var product);
            response.Items.Add(new OrderItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                Product = product
            });
        }

        return response;
    }
}