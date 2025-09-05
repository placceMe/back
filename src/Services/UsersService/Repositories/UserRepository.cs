using Microsoft.EntityFrameworkCore;
using UsersService.Data;
using UsersService.Models;

namespace UsersService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _context;

    public UserRepository(UsersDbContext context) => _context = context;

    public async Task<IEnumerable<User>> GetAllAsync() => await _context.Users.ToListAsync();

    public async Task<User?> GetByIdAsync(Guid id) => await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        await _context.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetAllWithPaginationAsync(int page, int pageSize)
    {
        var totalCount = await _context.Users.CountAsync();
        var users = await _context.Users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<User?> AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        var existing = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (existing == null) return false;
        _context.Entry(existing).CurrentValues.SetValues(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return false;
        user.State = UserState.Deleted;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddRegistrationUserAsync(RegistrationUser user)
    {
        await _context.RegistrationUsers.AddAsync(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RegistrationUser?> GetRegistrationUserByIdAsync(Guid id)
    {
        return await _context.RegistrationUsers.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> DeleteRegistrationUserAsync(Guid id)
    {
        var user = await _context.RegistrationUsers.FindAsync(id);
        if (user == null) return false;
        _context.RegistrationUsers.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<Guid?> GetRegistrationUserByEmailAsync(string email)
    {

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.State == UserState.Pending);
        if (user == null) return null;
        return user.Id;

    }
    public async Task<RegistrationUser?> GetRegistrationUserByTokenAsync(Guid token)
    {
        return await _context.RegistrationUsers.FirstOrDefaultAsync(u => u.ActivationCode == token);
    }
}
