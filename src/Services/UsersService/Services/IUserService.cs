using UsersService.Models;

namespace UsersService.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> SoftDeleteAsync(Guid id);
    Task<bool> MakeSellerAsync(Guid id);
}
