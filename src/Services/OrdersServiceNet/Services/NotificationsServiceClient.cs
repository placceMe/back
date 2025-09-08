using System.Text;
using System.Text.Json;
using Marketplace.Contracts.Orders;
using Marketplace.Contracts.Common;

namespace OrdersServiceNet.Services;

public class NotificationsServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationsServiceClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public NotificationsServiceClient(HttpClient httpClient, ILogger<NotificationsServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<bool> SendOrderCreatedEmailAsync(OrderCreatedEmailRequest request)
    {
        try
        {
            _logger.LogInformation("Sending order created email for order {OrderId} to {Email}",
                request.OrderId, request.To);

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentType!.CharSet = "utf-8";

            var response = await _httpClient.PostAsync("api/email/order-created", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Order created email sent successfully for order {OrderId}", request.OrderId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to send order created email for order {OrderId}. Status: {StatusCode}, Error: {Error}",
                request.OrderId, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order created email for order {OrderId}", request.OrderId);
            return false;
        }
    }
}

public class OrderCreatedEmailRequest
{
    public required string To { get; set; }
    public required string UserDisplayName { get; set; }
    public required string OrderId { get; set; }
    public required decimal TotalAmount { get; set; }
    public required List<OrderItemEmailInfo> Items { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Notes { get; set; }
}

public class OrderItemEmailInfo
{
    public required string ProductName { get; set; }
    public required int Quantity { get; set; }
    public required decimal Price { get; set; }
}

