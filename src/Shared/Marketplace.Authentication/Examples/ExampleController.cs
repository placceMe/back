using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

// ?????????????? shared authentication
using Marketplace.Authentication.Abstractions;

namespace ExampleService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExampleController : ControllerBase
{
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<ExampleController> _logger;

    public ExampleController(ISessionManager sessionManager, ILogger<ExampleController> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublicData()
    {
        // ????????? ???????? - ?? ???????? ??????????????
        return Ok(new { message = "???????? ????" });
    }

    [HttpPost]
    [Authorize] // ????????? ????????
    public async Task<IActionResult> CreateData([FromBody] object data)
    {
        try
        {
            // ????????? ?????????? ??? ??????????? ? JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var sessionId = User.FindFirst("session_id")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid user" });
            }

            // ??????????? ???????????? ????? (??????????? - middleware ??? ?? ??????)
            if (!string.IsNullOrEmpty(sessionId))
            {
                var sessionValidation = await _sessionManager.ValidateSessionByIdAsync(sessionId);
                if (!sessionValidation.IsValid)
                {
                    return Unauthorized(new { message = "Session expired" });
                }
            }

            _logger.LogInformation("User {UserId} ({UserEmail}) is creating data", userId, userEmail);

            // ??? ???? ??????-??????
            return Ok(new { 
                message = "???? ????????", 
                userId = userId,
                userEmail = userEmail 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating data");
            return StatusCode(500, new { message = "Server error" });
        }
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyData()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // ??????????? ????????? userId ? JWT ?????? ??????? shared authentication
        return Ok(new { 
            message = $"???? ??????????? {userId}",
            userId = userId,
            userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        });
    }
}