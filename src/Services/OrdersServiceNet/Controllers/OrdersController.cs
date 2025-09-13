using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Marketplace.Contracts.Orders;
using Marketplace.Contracts.Common;
using OrdersServiceNet.Services;
using System.Security.Claims;

namespace OrdersServiceNet.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Всі ендпоінти потребують авторизації
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Створити нове замовлення (тільки авторизований користувач)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(CreateOrderRequest request)
    {
        try
        {
            // Отримуємо ID поточного користувача з JWT токену
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userGuid))
            {
                _logger.LogWarning("Помилка отримання користувача або парсингу userId з токену");
                return Unauthorized(new { message = "Невірний токен авторизації" });
            }

            // Для безпеки: перевіряємо чи користувач створює замовлення для себе
            if (request.UserId != userGuid)
            {
                _logger.LogWarning("Користувач {CurrentUserId} намагається створити замовлення для {RequestUserId}",
                    currentUserId, request.UserId);
                return Forbid("Ви можете створювати замовлення тільки для себе");
            }

            var ordersResponse = await _orderService.CreateOrderAsync(request);

            _logger.LogInformation("Користувач {UserId} створив {OrdersCount} замовлення з загальною сумою {TotalAmount}",
                currentUserId, ordersResponse.OrdersCount, ordersResponse.TotalAmount);

            // Повертаємо створені замовлення
            if (ordersResponse.Orders.Count == 1)
            {
                // Якщо створено одне замовлення - повертаємо його як раніше
                var order = ordersResponse.Orders.First();
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, ordersResponse);
            }
            else
            {
                // Якщо створено кілька замовлень - повертаємо загальну відповідь
                return Ok(ordersResponse);
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Невірні дані для створення замовлення: {Error}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні замовлення");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Отримати замовлення за ID (тільки власник або адмін)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Перевіряємо права доступу
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Замовлення можуть переглядати власники або адміністратори
            if (order.UserId.ToString() != currentUserId && !userRoles.Contains("Admin"))
            {
                _logger.LogWarning("Користувач {UserId} намагається переглянути чуже замовлення {OrderId}",
                    currentUserId, id);
                return Forbid("Ви можете переглядати тільки свої замовлення");
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні замовлення {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Отримати замовлення користувача (тільки свої замовлення)
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrdersByUser(Guid userId)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Перевіряємо права доступу
            if (userId.ToString() != currentUserId && !userRoles.Contains("Admin"))
            {
                _logger.LogWarning("Користувач {CurrentUserId} намагається переглянути замовлення користувача {RequestedUserId}",
                    currentUserId, userId);
                return Forbid("Ви можете переглядати тільки свої замовлення");
            }

            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні замовлень користувача {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Отримати замовлення за ID продавця (тільки продавці)
    /// </summary>
    [HttpGet("by-seller/{id}")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrdersBySeller(Guid id)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Перевіряємо на те, що користувач переглядає замовлення власних товарів або адміністратор
            if (id.ToString() != currentUserId && !userRoles.Contains("Admin"))
            {
                _logger.LogWarning("Користувач {CurrentUserId} намагається переглянути замовлення продавця {SellerId}",
                    currentUserId, id);
                return Forbid("Ви можете переглядати тільки замовлення своїх товарів");
            }

            var orders = await _orderService.GetOrdersBySellerIdAsync(id);

            _logger.LogDebug("Отримано {Count} замовлень для продавця {SellerId}",
                orders.Count(), id);

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні замовлень продавця {SellerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Отримати мої замовлення (поточний користувач)
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetMyOrders()
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userGuid))
            {
                return Unauthorized(new { message = "???????? ????? ???????????" });
            }

            var orders = await _orderService.GetOrdersByUserIdAsync(userGuid);

            _logger.LogDebug("????????? {Count} ????????? ??? ??????????? {UserId}",
                orders.Count(), currentUserId);

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "??????? ??? ????????? ????????? ????????? ???????????");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// ???????? ??? ?????????? (?????? ??????????????)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")] // ?? ?????? ??????????????
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAllOrders()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("????????????? {UserId} ???????? ??? ??????????, ????????? {Count} ???????",
                currentUserId, orders.Count());

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "??????? ??? ????????? ???? ?????????");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// ??????????? ?????????? (?????? ?????????????? ??? ????????)
    /// </summary>
    [HttpPut("{id}/confirm")]
    public async Task<ActionResult<OrderResponse>> ConfirmOrder(Guid id)
    {
        try
        {
            // ???????? ????????? ?????????? ??? ????????? ????
            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // ?? ??????????? ?????: ??????? ?????????? ??? ?????
            if (existingOrder.UserId.ToString() != currentUserId)
            {
                _logger.LogWarning("?????????? {UserId} ??????????? ??????????? ???? ?????????? {OrderId}",
                    currentUserId, id);
                return Forbid("?? ?????? ?????????????? ?????? ???? ??????????");
            }

            var order = await _orderService.ConfirmOrderAsync(id);

            _logger.LogInformation("?????????? {OrderId} ???????????? ???????????? {UserId}", id, currentUserId);

            return order != null ? Ok(order) : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "??????? ??? ????????????? ?????????? {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// ????????? ?????????? (?????? ?????????????? ??? ????????)
    /// </summary>
    [HttpPut("{id}/reject")]
    public async Task<ActionResult<OrderResponse>> RejectOrder(Guid id)
    {
        try
        {
            // ???????? ????????? ?????????? ??? ????????? ????
            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // ?? ??????????? ?????: ??????? ?????????? ??? ?????
            // if (existingOrder.UserId.ToString() != currentUserId)
            // {
            //     _logger.LogWarning("?????????? {UserId} ??????????? ????????? ???? ?????????? {OrderId}",
            //         currentUserId, id);
            //     return Forbid("?? ?????? ????????? ?????? ???? ??????????");
            // }

            var order = await _orderService.RejectOrderAsync(id);

            _logger.LogInformation("?????????? {OrderId} ????????? ???????????? {UserId}", id, currentUserId);

            return order != null ? Ok(order) : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "??????? ??? ?????????? ?????????? {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// ???????? ?????????? (?????? ??????????????)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // ?? ?????? ?????????????? ?????? ????????
    public async Task<ActionResult> DeleteOrder(Guid id)
    {
        try
        {
            var success = await _orderService.DeleteOrderAsync(id);

            if (success)
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("?????????? {OrderId} ???????? ??????????????? {UserId}", id, currentUserId);
                return NoContent();
            }

            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "??????? ??? ????????? ?????????? {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

/// <summary>
/// Health check ????????? (????????? ??????)
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous] // ?? ????????? ?????? ??? ???????????
    public ActionResult Get()
    {
        return Ok(new { status = "healthy", service = "OrdersServiceNet", timestamp = DateTime.UtcNow });
    }
}
