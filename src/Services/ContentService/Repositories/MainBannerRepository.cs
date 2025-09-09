using ContentService.Data;
using ContentService.Models;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Repositories;

public class MainBannerRepository : IMainBannerRepository
{
    private readonly ContentDbContext _context;

    public MainBannerRepository(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MainBanner>> GetAllAsync()
    {
        return await _context.MainBanners
            .OrderBy(b => b.Order)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<MainBanner>> GetVisibleAsync()
    {
        return await _context.MainBanners
            .Where(b => b.IsVisible)
            .OrderBy(b => b.Order)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<MainBanner?> GetByIdAsync(int id)
    {
        return await _context.MainBanners.FindAsync(id);
    }

    public async Task<MainBanner> CreateAsync(MainBanner mainBanner)
    {
        mainBanner.CreatedAt = DateTime.UtcNow;
        mainBanner.UpdatedAt = DateTime.UtcNow;

        _context.MainBanners.Add(mainBanner);
        await _context.SaveChangesAsync();
        return mainBanner;
    }

    public async Task<MainBanner> UpdateAsync(MainBanner mainBanner)
    {
        mainBanner.UpdatedAt = DateTime.UtcNow;

        _context.MainBanners.Update(mainBanner);
        await _context.SaveChangesAsync();
        return mainBanner;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var banner = await _context.MainBanners.FindAsync(id);
        if (banner == null)
            return false;

        _context.MainBanners.Remove(banner);
        await _context.SaveChangesAsync();
        return true;
    }
}
