using UsersService.Models;

namespace UsersService.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> SoftDeleteAsync(Guid id);
}
