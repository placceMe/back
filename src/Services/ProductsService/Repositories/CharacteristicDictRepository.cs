using ProductsService.Models;
using ProductsService.Data;
using Microsoft.EntityFrameworkCore;

namespace ProductsService.Repositories;

public class CharacteristicDictRepository : ICharacteristicDictRepository
{
    private readonly ProductsDBContext _context;
    public CharacteristicDictRepository(ProductsDBContext context)
    {
        _context = context;
    }
    public async Task CreateCharacteristicDictAsync(CharacteristicDict dict)
    {
        await _context.Set<CharacteristicDict>().AddAsync(dict);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> UpdateCharacteristicDictAsync(Guid id, CharacteristicDict dict)
    {
        var existing = await _context.Set<CharacteristicDict>().FirstOrDefaultAsync(d => d.Id == id);
        if (existing == null) return false;
        existing.Name = dict.Name;
        existing.CategoryId = dict.CategoryId;
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> DeleteCharacteristicDictAsync(Guid id)
    {
        var existing = await _context.Set<CharacteristicDict>().FirstOrDefaultAsync(d => d.Id == id);
        if (existing == null) return false;
        _context.Set<CharacteristicDict>().Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<CharacteristicDict?> GetCharacteristicDictByIdAsync(Guid id)
    {
        return await _context.Set<CharacteristicDict>().FirstOrDefaultAsync(d => d.Id == id);
    }
    public async Task<IEnumerable<CharacteristicDict>> GetCharacteristicDictsByCategoryIdAsync(Guid categoryId)
    {
        return await _context.Set<CharacteristicDict>()
            .Where(d => d.CategoryId == categoryId)
            .ToListAsync();
    }
    public async Task<IEnumerable<CharacteristicDict>> GetAllCharacteristicDictsAsync()
    {
        return await _context.Set<CharacteristicDict>().ToListAsync();
    }
}
