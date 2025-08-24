using UsersService.Models;

namespace UsersService.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> AddAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> SoftDeleteAsync(Guid id);
    Task<bool> AddRegistrationUserAsync(RegistrationUser user);
    Task<RegistrationUser?> GetRegistrationUserByIdAsync(Guid id);
    Task<bool> DeleteRegistrationUserAsync(Guid id);
    Task<Guid?> GetRegistrationUserByEmailAsync(string email);
    Task<RegistrationUser?> GetRegistrationUserByTokenAsync(Guid token);
}
