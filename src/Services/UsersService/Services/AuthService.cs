using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using UsersService.DTOs;
using UsersService.Models;
using UsersService.Repositories;
using System.Security.Cryptography;

namespace UsersService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IRedisAuthStore _redisAuthStore;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger, IRedisAuthStore redisAuthStore)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
        _redisAuthStore = redisAuthStore;
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

            // Generate access token and refresh token
            var deviceId = GenerateDeviceId();
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            // Store refresh token in Redis
            await _redisAuthStore.StoreRefreshTokenAsync(
                user.Id,
                deviceId,
                refreshToken,
                refreshTokenExpiry,
                CancellationToken.None
            );

            return new AuthResponse
            {
                Success = true,
                Message = "Успішний вхід",
                User = userInfo,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                DeviceId = deviceId
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

    public async Task<AuthResponse> LogoutAsync(string jti, DateTimeOffset accessTokenExpiry)
    {
        try
        {
            // Add JWT to blacklist in Redis
            await _redisAuthStore.RevokeJtiAsync(jti, accessTokenExpiry, CancellationToken.None);

            return new AuthResponse { Success = true, Message = "Успішний вихід" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при виході з системи");
            return new AuthResponse { Success = false, Message = "Помилка сервера" };
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(Guid userId, string deviceId, string oldRefreshToken)
    {
        try
        {
            var newRefreshToken = GenerateRefreshToken();
            var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            var isValid = await _redisAuthStore.ValidateAndRotateRefreshTokenAsync(
                userId,
                deviceId,
                oldRefreshToken,
                newRefreshToken,
                newRefreshTokenExpiry,
                CancellationToken.None
            );

            if (!isValid)
            {
                return new AuthResponse { Success = false, Message = "Невірний refresh token" };
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.State != UserState.Active)
            {
                return new AuthResponse { Success = false, Message = "Користувач не знайдений або неактивний" };
            }

            var newAccessToken = GenerateJwtToken(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Токен оновлено",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні токена для користувача {UserId}", userId);
            return new AuthResponse { Success = false, Message = "Помилка сервера" };
        }
    }

    public string GenerateJwtToken(User user)
    {
        var jti = Guid.NewGuid().ToString();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default-secret-key-for-development-only-min-32-chars"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, jti)
        };

        // Add roles as separate claims
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "UsersService",
            audience: _configuration["Jwt:Audience"] ?? "MarketplaceClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1), // Shorter expiry for access tokens
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private string GenerateDeviceId()
    {
        return Guid.NewGuid().ToString();
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

    public async Task<AuthResponse> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Користувач не знайдений"
                };
            }

            // Перевіряємо поточний пароль
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Невірний поточний пароль"
                };
            }

            // Валідація нового пароля
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Новий пароль має містити принаймні 6 символів"
                };
            }

            // Хешуємо новий пароль і оновлюємо користувача
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Пароль успішно змінено"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при зміні пароля для користувача {UserId}", userId);
            return new AuthResponse
            {
                Success = false,
                Message = "Помилка при зміні пароля"
            };
        }
    }
}