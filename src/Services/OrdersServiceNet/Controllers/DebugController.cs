using Microsoft.AspNetCore.Mvc;

namespace OrdersServiceNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DebugController> _logger;

    public DebugController(IConfiguration configuration, ILogger<DebugController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Debug endpoint to check JWT configuration
    /// </summary>
    [HttpGet("jwt-config")]
    public ActionResult GetJwtConfig()
    {
        var jwtSecretKey = _configuration["Jwt:SecretKey"] ?? "not-set";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "not-set";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "not-set";

        var config = new
        {
            SecretKeyLength = jwtSecretKey.Length,
            SecretKeyFirst10Chars = jwtSecretKey.Length > 10 ? jwtSecretKey.Substring(0, 10) + "..." : jwtSecretKey,
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "not-set",
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("JWT Configuration requested: {@Config}", config);

        return Ok(config);
    }

    /// <summary>
    /// Debug endpoint to check claims from JWT token
    /// </summary>
    [HttpGet("jwt-claims")]
    public ActionResult GetJwtClaims()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new { message = "No JWT token provided or invalid" });
        }

        var claims = User.Claims.Select(c => new
        {
            Type = c.Type,
            Value = c.Value
        }).ToList();

        var result = new
        {
            IsAuthenticated = User.Identity.IsAuthenticated,
            AuthenticationType = User.Identity.AuthenticationType,
            Name = User.Identity.Name,
            Claims = claims,
            UserIdFromClaims = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            RolesFromClaims = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList(),
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("JWT Claims parsed: {@Claims}", result);

        return Ok(result);
    }
}