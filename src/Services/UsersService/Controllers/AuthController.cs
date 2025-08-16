using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UsersService.Services;
using UsersService.DTOs;
using UsersService.Repositories;
using System.IdentityModel.Tokens.Jwt;

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
    public async Task<IActionResult> Logout()
    {
        try
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                token = HttpContext.Request.Cookies["authToken"];
            }

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Токен не знайдено" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);
            var jti = jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var exp = jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)?.Value;

            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(exp))
            {
                return BadRequest(new { message = "Невірний токен" });
            }

            var expiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp));
            var result = await _authService.LogoutAsync(jti, expiry);

            if (result.Success)
            {
                Response.Cookies.Delete("authToken");
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при виході з системи");
            return StatusCode(500, new { message = "Внутрішня помилка сервера" });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.UserId, request.DeviceId, request.RefreshToken);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні токена");
            return StatusCode(500, new { message = "Внутрішня помилка сервера" });
        }
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