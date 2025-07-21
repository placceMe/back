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

            // TODO: Add BCrypt password verification
            if (request.Password != user.Password) // Temporary simple check
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

            // TODO: Add BCrypt password hashing
            var user = new User
            {
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email,
                Password = request.Password, // TODO: Hash this
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
        // TODO: Implement JWT token generation when packages are added
        return "temporary-token";
    }

    public bool ValidateToken(string token)
    {
        // TODO: Implement JWT token validation when packages are added
        return !string.IsNullOrEmpty(token);
    }

    public async Task<User?> GetCurrentUserAsync(string token)
    {
        // TODO: Implement proper token parsing when JWT packages are added
        return null;
    }
}