using UsersService.Data;
using UsersService.Models;
using UsersService.Repositories;

namespace UsersService.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;
    private readonly INotificationServiceClient _notificationServiceClient;

    public UserService(IUserRepository repository, ILogger<UserService> logger, INotificationServiceClient notificationServiceClient)
    {
        _repository = repository;
        _logger = logger;
        _notificationServiceClient = notificationServiceClient;
    }

    public Task<IEnumerable<User>> GetAllAsync() => _repository.GetAllAsync();

    public Task<User?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

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

    public async Task<bool> RegisterUserAsync(RegistrationUser user)
    {
        if (user == null) return false;

        user.ActivationCodeExpiresAt = DateTime.UtcNow.AddHours(24); // Код действителен 24 часа

        var result = await _repository.AddRegistrationUserAsync(user);


        if (!result)
        {
            _logger.LogWarning("Failed to register user {UserId}", user.Id);
        }
        if (result)
        {
            await _notificationServiceClient.SendRegistrationNotificationAsync(user.Email, user.Name);
            _logger.LogInformation("Sent registration notification for user {UserId}", user.Id);
        }
        return result;
    }

    public async Task<bool> ConfirmUserAsync(Guid userId, string token)
    {
        var user = await _repository.GetRegistrationUserByIdAsync(userId);
        if (user == null) return false;

        if (user.ActivationCode != token || user.ActivationCodeExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Failed to confirm user {UserId}: invalid or expired token", userId);
            return false;
        }

        var newUser = new User
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            Password = user.Password,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            Roles = new List<string> { Role.User },
            State = UserState.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(newUser);
        await _repository.DeleteRegistrationUserAsync(userId);
        _logger.LogInformation("User {UserId} confirmed and added to main users", userId);
        return true;


    }
}
