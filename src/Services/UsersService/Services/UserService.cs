using UsersService.Models;
using UsersService.Repositories;

namespace UsersService.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository) => _repository = repository;

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
}
