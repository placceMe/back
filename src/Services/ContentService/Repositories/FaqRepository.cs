using Microsoft.EntityFrameworkCore;
using ContentService.Data;
using ContentService.Models;

namespace ContentService.Repositories;

public interface IFaqRepository
{
    Task<IEnumerable<Faq>> GetAllAsync();
    Task<IEnumerable<Faq>> GetVisibleAsync();
    Task<Faq?> GetByIdAsync(int id);
    Task<Faq> CreateAsync(Faq faq);
    Task<Faq> UpdateAsync(Faq faq);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public class FaqRepository : IFaqRepository
{
    private readonly ContentDbContext _context;

    public FaqRepository(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Faq>> GetAllAsync()
    {
        return await _context.Faqs
            .OrderBy(f => f.Order)
            .ThenBy(f => f.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Faq>> GetVisibleAsync()
    {
        return await _context.Faqs
            .Where(f => f.IsVisible)
            .OrderBy(f => f.Order)
            .ThenBy(f => f.Id)
            .ToListAsync();
    }

    public async Task<Faq?> GetByIdAsync(int id)
    {
        return await _context.Faqs.FindAsync(id);
    }

    public async Task<Faq> CreateAsync(Faq faq)
    {
        faq.CreatedAt = DateTime.UtcNow;
        faq.UpdatedAt = DateTime.UtcNow;

        _context.Faqs.Add(faq);
        await _context.SaveChangesAsync();
        return faq;
    }

    public async Task<Faq> UpdateAsync(Faq faq)
    {
        faq.UpdatedAt = DateTime.UtcNow;

        _context.Faqs.Update(faq);
        await _context.SaveChangesAsync();
        return faq;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var faq = await _context.Faqs.FindAsync(id);
        if (faq == null)
            return false;

        _context.Faqs.Remove(faq);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Faqs.AnyAsync(f => f.Id == id);
    }
}
