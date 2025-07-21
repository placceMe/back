using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using UsersService.DTOs;
using UsersService.Models;
using UsersService.Repositories;

namespace UsersService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || user.State == UserState.Deleted)
            {
                return new AuthResponse { Success = false, Message = "Невірний email або пароль" };
            }

            if (user.State == UserState.Blocked)
            {
                return new AuthResponse { Success = false, Message = "Акаунт заблокований" };
            }

            // Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return new AuthResponse { Success = false, Message = "Невірний email або пароль" };
            }

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Roles = user.Roles
            };

            return new AuthResponse
            {
                Success = true,
                Message = "Успішний вхід",
                User = userInfo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при авторизації користувача {Email}", request.Email);
            return new AuthResponse { Success = false, Message = "Помилка сервера" };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResponse { Success = false, Message = "Користувач з таким email вже існує" };
            }

            // Hash password using BCrypt
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email,
                Password = hashedPassword,
                Phone = request.Phone,
                Roles = new List<string> { Role.User }
            };

            await _userRepository.AddAsync(user);

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Roles = user.Roles
            };

            return new AuthResponse
            {
                Success = true,
                Message = "Реєстрація успішна",
                User = userInfo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при реєстрації користувача {Email}", request.Email);
            return new AuthResponse { Success = false, Message = "Помилка сервера" };
        }
    }

    public Task<AuthResponse> LogoutAsync()
    {
        return Task.FromResult(new AuthResponse { Success = true, Message = "Успішний вихід" });
    }

    public string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default-secret-key-for-development-only-min-32-chars"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email)
        };

        // Add roles as separate claims
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "UsersService",
            audience: _configuration["Jwt:Audience"] ?? "MarketplaceClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default-secret-key-for-development-only-min-32-chars");

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "UsersService",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "MarketplaceClient",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<User?> GetCurrentUserAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);

            var userIdClaim = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            return await _userRepository.GetByIdAsync(userId);
        }
        catch
        {
            return null;
        }
    }
}