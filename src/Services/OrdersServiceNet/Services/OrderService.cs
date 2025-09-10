using Marketplace.Contracts.Orders;
using Marketplace.Contracts.Common;
using OrdersServiceNet.Models;
using OrdersServiceNet.Repositories;

namespace OrdersServiceNet.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ProductsServiceClient _productsClient;
    private readonly UsersServiceClient _usersClient;
    private readonly NotificationsServiceClient _notificationsClient;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        ProductsServiceClient productsClient,
        UsersServiceClient usersClient,
        NotificationsServiceClient notificationsClient,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productsClient = productsClient;
        _usersClient = usersClient;
        _notificationsClient = notificationsClient;
        _logger = logger;
    }

    public async Task<CreateOrdersResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        // Validate user exists and get user info
        var user = await _usersClient.GetUserByIdAsync(request.UserId);
        if (user == null)
            throw new ArgumentException($"User {request.UserId} not found");

        // Get all unique product IDs
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();

        // Fetch all products in one request
        var productsDict = await _productsClient.GetProductsManyAsync(productIds);

        // Get product-seller mapping
        var productSellerMap = await _productsClient.GetProductSellerMappingAsync(productIds);

        // Group items by seller
        var itemsBySeller = new Dictionary<Guid, List<OrderItemRequest>>();

        foreach (var item in request.Items)
        {
            if (!productsDict.TryGetValue(item.ProductId, out var product))
                throw new ArgumentException($"Product {item.ProductId} not found");

            if (!productSellerMap.TryGetValue(item.ProductId, out var sellerId))
                throw new ArgumentException($"Seller not found for product {item.ProductId}");

            if (!itemsBySeller.ContainsKey(sellerId))
                itemsBySeller[sellerId] = new List<OrderItemRequest>();

            itemsBySeller[sellerId].Add(item);
        }

        var createdOrders = new List<OrderResponse>();
        decimal totalAmount = 0;

        // Create separate order for each seller
        foreach (var sellerItems in itemsBySeller)
        {
            var sellerId = sellerItems.Key;
            var items = sellerItems.Value;

            var orderItems = new List<OrderItem>();
            decimal orderAmount = 0;

            // Validate products and calculate total for this seller
            foreach (var item in items)
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
                orderAmount += product.Price * item.Quantity;
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                TotalAmount = orderAmount,
                Status = OrderStatus.Pending,
                Notes = request.Notes,
                DeliveryAddress = request.DeliveryAddress,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            var createdOrder = await _orderRepository.CreateOrderAsync(order);
            _logger.LogInformation("Order {OrderId} created for user {UserId} with seller {SellerId}",
                createdOrder.Id, createdOrder.UserId, sellerId);

            var orderResponse = await MapToOrderResponseAsync(createdOrder);
            createdOrders.Add(orderResponse);
            totalAmount += orderAmount;

            // Send order confirmation email for each order (асинхронно, без блокування)
            _ = Task.Run(async () =>
            {
                try
                {
                    var emailRequest = new OrderCreatedEmailRequest
                    {
                        To = user.Email,
                        UserDisplayName = !string.IsNullOrEmpty(user.Name) || !string.IsNullOrEmpty(user.Surname)
                            ? $"{user.Name} {user.Surname}".Trim()
                            : "Користувач",
                        OrderId = createdOrder.Id.ToString(),
                        TotalAmount = createdOrder.TotalAmount,
                        DeliveryAddress = createdOrder.DeliveryAddress,
                        Notes = createdOrder.Notes,
                        Items = createdOrder.OrderItems.Select(item =>
                        {
                            productsDict.TryGetValue(item.ProductId, out var product);
                            return new OrderItemEmailInfo
                            {
                                ProductName = product?.Name ?? $"Product {item.ProductId}",
                                Quantity = item.Quantity,
                                Price = item.Price
                            };
                        }).ToList()
                    };

                    var emailSent = await _notificationsClient.SendOrderCreatedEmailAsync(emailRequest);

                    if (emailSent)
                    {
                        _logger.LogInformation("Order confirmation email sent successfully for order {OrderId} to {Email}",
                            createdOrder.Id, user.Email);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send order confirmation email for order {OrderId} to {Email}",
                            createdOrder.Id, user.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending order confirmation email for order {OrderId}", createdOrder.Id);
                }
            });
        }

        var response = new CreateOrdersResponse
        {
            Orders = createdOrders,
            TotalAmount = totalAmount,
            OrdersCount = createdOrders.Count,
            Message = createdOrders.Count > 1
                ? $"Ваші товари від {createdOrders.Count} різних продавців були розділені на окремі замовлення."
                : "Замовлення успішно створено."
        };

        _logger.LogInformation("Created {OrdersCount} orders with total amount {TotalAmount} for user {UserId}",
            response.OrdersCount, response.TotalAmount, request.UserId);

        return response;
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

    public async Task<IEnumerable<OrderResponse>> GetOrdersBySellerIdAsync(Guid sellerId)
    {
        // Get all orders first to collect all product IDs
        var allOrders = await _orderRepository.GetAllOrdersAsync();
        var allProductIds = allOrders.SelectMany(o => o.OrderItems.Select(oi => oi.ProductId)).Distinct().ToList();

        if (!allProductIds.Any())
            return Enumerable.Empty<OrderResponse>();

        // Get product-seller mapping
        var productSellerMap = await _productsClient.GetProductSellerMappingAsync(allProductIds);

        // Get orders that contain products from the specified seller
        var sellerOrders = await _orderRepository.GetOrdersBySellerIdAsync(sellerId, productSellerMap);
        var responses = new List<OrderResponse>();

        foreach (var order in sellerOrders)
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

    public async Task<OrderResponse?> ConfirmOrderAsync(Guid id)
    {
        var order = await _orderRepository.GetOrderByIdAsync(id);
        if (order == null) return null;

        order.Status = OrderStatus.Confirmed;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateOrderAsync(order);

        // Update product quantities after successful order confirmation
        foreach (var item in order.OrderItems)
        {
            // Decrease product quantity by ordered amount
            await _productsClient.ChangeProductQuantityAsync(item.ProductId, "minus", item.Quantity);
        }

        _logger.LogInformation("Order {OrderId} confirmed and product quantities updated", id);

        return await MapToOrderResponseAsync(order);
    }

    public async Task<OrderResponse?> RejectOrderAsync(Guid id)
    {
        var order = await _orderRepository.GetOrderByIdAsync(id);
        if (order == null) return null;

        order.Status = OrderStatus.Rejected;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateOrderAsync(order);
        return await MapToOrderResponseAsync(order);
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
