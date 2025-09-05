using Microsoft.EntityFrameworkCore;
using UsersService.Data;
using UsersService.Models;

namespace UsersService.Repositories;

public class SalerInfoRepository : ISalerInfoRepository
{
    private readonly UsersDbContext _context;

    public SalerInfoRepository(UsersDbContext context) => _context = context;

    public async Task<IEnumerable<SalerInfo>> GetAllAsync() =>
        await _context.SalerInfos.Include(s => s.User).ToListAsync();

    public async Task<SalerInfo?> GetByIdAsync(Guid id) =>
        await _context.SalerInfos.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<IEnumerable<SalerInfo>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        await _context.SalerInfos.Include(s => s.User).Where(s => ids.Contains(s.Id)).ToListAsync();

    public async Task<SalerInfo?> GetByUserIdAsync(Guid userId) =>
        await _context.SalerInfos.FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task AddAsync(SalerInfo salerInfo)
    {
        salerInfo.CreatedAt = DateTime.UtcNow;
        salerInfo.UpdatedAt = DateTime.UtcNow;
        await _context.SalerInfos.AddAsync(salerInfo);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(SalerInfo salerInfo)
    {
        var existing = await _context.SalerInfos.FirstOrDefaultAsync(s => s.Id == salerInfo.Id);
        if (existing == null) return false;

        salerInfo.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existing).CurrentValues.SetValues(salerInfo);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var salerInfo = await _context.SalerInfos.FirstOrDefaultAsync(s => s.Id == id);
        if (salerInfo == null) return false;

        _context.SalerInfos.Remove(salerInfo);
        await _context.SaveChangesAsync();
        return true;
    }
}