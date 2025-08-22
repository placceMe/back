using UsersService.Data;
using UsersService.Models;
using UsersService.Repositories;

namespace UsersService.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
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
}
