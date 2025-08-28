using Microsoft.AspNetCore.Mvc;
using OrdersServiceNet.DTOs;
using OrdersServiceNet.Services;

namespace OrdersServiceNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return order != null ? Ok(order) : NotFound();
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrdersByUser(Guid userId)
    {
        var orders = await _orderService.GetOrdersByUserIdAsync(userId);
        return Ok(orders);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpPut("{id}/confirm")]
    public async Task<ActionResult<OrderResponse>> ConfirmOrder(Guid id)
    {
        var order = await _orderService.ConfirmOrderAsync(id);
        return order != null ? Ok(order) : NotFound();
    }

    [HttpPut("{id}/reject")]
    public async Task<ActionResult<OrderResponse>> RejectOrder(Guid id)
    {
        var order = await _orderService.RejectOrderAsync(id);
        return order != null ? Ok(order) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteOrder(Guid id)
    {
        var success = await _orderService.DeleteOrderAsync(id);
        return success ? NoContent() : NotFound();
    }
}

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult Get()
    {
        return Ok(new { status = "healthy", service = "OrdersServiceNet", timestamp = DateTime.UtcNow });
    }
}