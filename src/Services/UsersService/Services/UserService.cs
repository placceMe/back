using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using UsersService.DTOs;
using UsersService.Models;
using UsersService.Repositories;
using System.Security.Cryptography;
using System.Text.Json;

namespace UsersService.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ISalerInfoService _salerInfoService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserService> _logger;
    private readonly INotificationServiceClient _notificationServiceClient;
    private readonly IRedisAuthStore _redisAuthStore;

    public UserService(
        IUserRepository repository,
        ISalerInfoService salerInfoService,
        IConfiguration configuration,
        ILogger<UserService> logger,
        INotificationServiceClient notificationServiceClient,
        IRedisAuthStore redisAuthStore
        )
    {
        _repository = repository;
        _salerInfoService = salerInfoService;
        _configuration = configuration;
        _logger = logger;
        _notificationServiceClient = notificationServiceClient;
        _redisAuthStore = redisAuthStore;
    }

    // User Management Methods
    public Task<IEnumerable<User>> GetAllAsync() => _repository.GetAllAsync();

    public Task<User?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    public Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> ids) => _repository.GetByIdsAsync(ids);

    public Task CreateAsync(User user) => _repository.AddAsync(user);

    public async Task<bool> UpdateAsync(User user)
    {
        return await _repository.UpdateAsync(user);
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        return await _repository.SoftDeleteAsync(id);
    }

    public async Task<bool> MakeSellerAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        _logger.LogInformation("Making user {UserId} a saler", id);
        if (user == null) return false;

        if (!user.Roles.Contains(Role.Saler))
        {
            user.Roles.Add(Role.Saler);
            await _repository.UpdateAsync(user);

        }
        return true;
    }

    public async Task<bool> UpdateRolesAsync(Guid userId, List<string> roles)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user == null)
            return false;

        user.Roles = roles;
        await _repository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ChangeStateAsync(Guid userId, string newState)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user == null)
            return false;

        user.State = newState;
        await _repository.UpdateAsync(user);
        _logger.LogInformation("Changed state of user {UserId} to {NewState}", userId, newState);
        return true;
    }

    public async Task<bool> ConfirmUserAsync(Guid token)
    {
        var registrationUser = await _repository.GetRegistrationUserByTokenAsync(token);
        if (registrationUser == null) return false;

        if (registrationUser.ActivationCode != token || registrationUser.ActivationCodeExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Failed to confirm user {UserId}: invalid or expired token", registrationUser.Id);
            return false;
        }

        // Change user state to Active in main Users table
        await this.ChangeStateAsync(registrationUser.Id, UserState.Active);

        // Remove from RegistrationUsers table only
        await _repository.DeleteRegistrationUserAsync(registrationUser.Id);

        _logger.LogInformation("User {UserId} confirmed and activated", registrationUser.Id);
        return true;
    }

    // Authentication Methods
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Спроба входу: {Email}", request.Email);
            var user = await _repository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Користувача не знайдено: {Email}", request.Email);
                return new AuthResponse { Success = false, Message = "Невірний email або пароль" };
            }
            _logger.LogInformation("Стан користувача {Email}: {State}", request.Email, user.State);
            if (user.State == UserState.Deleted)
            {
                _logger.LogWarning("Користувач видалений: {Email}", request.Email);
                return new AuthResponse { Success = false, Message = "Невірний email або пароль" };
            }
            if (user.State == UserState.Blocked)
            {
                _logger.LogWarning("Користувач заблокований: {Email}", request.Email);
                return new AuthResponse { Success = false, Message = "Акаунт заблокований" };
            }
            // Verify password using BCrypt
            var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            _logger.LogInformation("Результат перевірки пароля для {Email}: {Result}", request.Email, passwordValid);
            if (!passwordValid)
            {
                _logger.LogWarning("Невірний пароль для {Email}", request.Email);
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
            var existingUser = await _repository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResponse { Success = false, Message = "Користувач з таким email вже існує" };
            }
            var existNotConfirmedUser = await _repository.GetRegistrationUserByEmailAsync(request.Email);
            if (existNotConfirmedUser != null)
            {
                return new AuthResponse { Success = false, Message = "Користувач з таким email вже існує. Підтвердіть реєстрацію." };
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
                Roles = new List<string> { Role.User },
                State = UserState.Pending,
            };
            var registrationUser = new RegistrationUser
            {
                Id = user.Id
            };
            var newUser = await _repository.AddAsync(user);

            var result = await _repository.AddRegistrationUserAsync(registrationUser);

            if (!result)
            {
                _logger.LogWarning("Failed to register user {UserId}", user.Id);
            }
            if (result)
            {

                var baseUrl = _configuration["ASPNETCORE_BASE_URL"] ?? "http://localhost:5002";
                var registerUrl = $"{baseUrl}/api/auth/confirm-registration?token={registrationUser.ActivationCode}";

                await _notificationServiceClient.SendRegistrationNotificationAsync(user.Email, user.Name, registerUrl);
                _logger.LogInformation("Sent registration notification for user {UserId}", user.Id);
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
                Message = "Реєстрація успішна",
                // User = userInfo
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

            var user = await _repository.GetByIdAsync(userId);
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

            return await _repository.GetByIdAsync(userId);
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
            var user = await _repository.GetByIdAsync(userId);
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
            await _repository.UpdateAsync(user);

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

    public async Task<AuthResponse> ForgotPasswordAsync(string email)
    {
        try
        {
            var user = await _repository.GetByEmailAsync(email);
            if (user == null || user.State != UserState.Active)
            {
                // Повертаємо успіх навіть якщо користувач не знайдений (безпека)
                return new AuthResponse
                {
                    Success = true,
                    Message = "Якщо користувач з таким email існує, на нього буде відправлено лист з інструкціями"
                };
            }

            // Генеруємо токен скидання
            var resetToken = GenerateSecureToken();
            var expiry = TimeSpan.FromHours(1); // Токен дійсний 1 годину
            var resetUrl = "";

            // Зберігаємо токен в Redis
            await _redisAuthStore.StorePasswordResetTokenAsync(resetToken, user.Id, expiry);

            // Відправляємо email через NotificationsService
            await SendPasswordResetEmailAsync(user.Email, user.Name, resetToken);

            return new AuthResponse
            {
                Success = true,
                Message = "Якщо користувач з таким email існує, на нього буде відправлено лист з інструкціями"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при скиданні паролю для email {Email}", email);
            return new AuthResponse
            {
                Success = false,
                Message = "Помилка сервера"
            };
        }
    }

    public async Task<AuthResponse> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            // Валідуємо токен і отримуємо userId з Redis
            var userId = await _redisAuthStore.ValidatePasswordResetTokenAsync(token);

            if (userId == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Невірний або прострочений токен"
                };
            }

            var user = await _repository.GetByIdAsync(userId.Value);
            if (user == null || user.State != UserState.Active)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Користувач не знайдений"
                };
            }

            // Валідація нового пароля
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Пароль має містити принаймні 6 символів"
                };
            }

            // Оновлюємо пароль
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _repository.UpdateAsync(user);

            // Видаляємо використаний токен з Redis
            await _redisAuthStore.RevokePasswordResetTokenAsync(token);

            // Відкликаємо всі refresh токени для цього користувача
            await _redisAuthStore.RevokeAllUserTokensAsync(user.Id);

            return new AuthResponse
            {
                Success = true,
                Message = "Пароль успішно змінено"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при зміні паролю з токеном {Token}", token);
            return new AuthResponse
            {
                Success = false,
                Message = "Помилка сервера"
            };
        }
    }

    private string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private async Task SendPasswordResetEmailAsync(string email, string userName, string resetToken)
    {
        try
        {
            var notificationsServiceUrl = _configuration["NotificationsService:BaseUrl"];
            if (string.IsNullOrEmpty(notificationsServiceUrl))
            {
                _logger.LogWarning("NotificationsService URL не налаштований");
                return;
            }

            var frontendUrl = _configuration["Frontend:BaseUrl"];
            var resetUrl = $"{frontendUrl}/reset-password?token={resetToken}";

            using var httpClient = new HttpClient();
            var request = new
            {
                To = email,
                UserDisplayName = userName,
                ResetUrl = resetUrl
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{notificationsServiceUrl}api/email/password-reset", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Помилка відправки email: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при відправці email для скидання паролю");
        }
    }

    public async Task<UserInfoWithSellerInfo?> GetWithSellerInfoAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null) return null;

        var sellerInfo = await _salerInfoService.GetByUserIdAsync(id);
        return new UserInfoWithSellerInfo
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            Roles = user.Roles,
            SalerInfo = sellerInfo != null ? new SalerInfoResponseDto
            {
                Id = sellerInfo.Id,
                Description = sellerInfo.Description,
                CompanyName = sellerInfo.CompanyName,
                Schedule = sellerInfo.Schedule
            } : null
        };
    }

    public async Task<(IEnumerable<UserInfoWithSellerInfo> Users, int TotalCount)> GetAllWithSellerInfoAsync(int page = 1, int pageSize = 10)
    {
        var (users, totalCount) = await _repository.GetAllWithPaginationAsync(page, pageSize);
        var usersWithSellerInfo = new List<UserInfoWithSellerInfo>();

        foreach (var user in users)
        {
            var sellerInfo = await _salerInfoService.GetByUserIdAsync(user.Id);
            usersWithSellerInfo.Add(new UserInfoWithSellerInfo
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Roles = user.Roles,
                SalerInfo = sellerInfo != null ? new SalerInfoResponseDto
                {
                    Id = sellerInfo.Id,
                    Description = sellerInfo.Description,
                    CompanyName = sellerInfo.CompanyName,
                    Schedule = sellerInfo.Schedule,
                    UserId = sellerInfo.UserId,
                    CreatedAt = sellerInfo.CreatedAt,
                    UpdatedAt = sellerInfo.UpdatedAt,
                    Contacts = sellerInfo.Contacts.Select(c => new ContactDto
                    {
                        Type = c.Type,
                        Value = c.Value
                    }).ToList()
                } : null
            });
        }

        return (usersWithSellerInfo, totalCount);
    }
}
