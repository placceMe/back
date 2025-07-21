using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UsersService.DTOs;
using UsersService.Services;
using UsersService.Repositories;

namespace UsersService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IUserRepository userRepository, ILogger<AuthController> logger)
    {
        _authService = authService;
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result.Success && result.User != null)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user != null)
            {
                var token = _authService.GenerateJwtToken(user);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(24),
                    Path = "/"
                };

                Response.Cookies.Append("authToken", token, cookieOptions);

                _logger.LogInformation("Користувач {Email} успішно авторизувався", request.Email);
            }
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result.Success && result.User != null)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user != null)
            {
                var token = _authService.GenerateJwtToken(user);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(24),
                    Path = "/"
                };

                Response.Cookies.Append("authToken", token, cookieOptions);

                _logger.LogInformation("Новий користувач зареєстрований: {Email}", request.Email);
            }
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> Logout()
    {
        Response.Cookies.Delete("authToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        var result = await _authService.LogoutAsync();
        _logger.LogInformation("Користувач вийшов з системи");

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        if (!Request.Cookies.TryGetValue("authToken", out var token))
        {
            return Unauthorized(new { Message = "Токен не знайдено" });
        }

        var user = await _authService.GetCurrentUserAsync(token);
        if (user == null)
        {
            return Unauthorized(new { Message = "Невалідний токен" });
        }

        var userInfo = new UserInfo
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            Roles = user.Roles
        };

        return Ok(userInfo);
    }

    [HttpPost("validate")]
    public ActionResult ValidateToken()
    {
        if (!Request.Cookies.TryGetValue("authToken", out var token))
        {
            return Ok(new { Valid = false, Message = "Токен не знайдено" });
        }

        var isValid = _authService.ValidateToken(token);
        return Ok(new { Valid = isValid });
    }
}