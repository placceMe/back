using ProductsService.Models;
using ProductsService.Data;
using Marketplace.Contracts.Products;
using Marketplace.Contracts.Files;
using Marketplace.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using ProductsService.Repositories.Interfaces;


namespace ProductsService.Repositories;

public class CharacteristicRepository : ICharacteristicRepository
{
    private readonly ProductsDBContext _context;
    public CharacteristicRepository(ProductsDBContext context)
    {
        _context = context;
    }
    public async Task CreateCharacteristicAsync(Characteristic characteristic)
    {
        await _context.Set<Characteristic>().AddAsync(characteristic);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> UpdateCharacteristicAsync(UpdateCharacteristicDto characteristic)
    {
        var existing = await _context.Set<Characteristic>().FirstOrDefaultAsync(c => c.Id == characteristic.Id);
        if (existing == null) return false;
        existing.Value = characteristic.Value;
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> DeleteCharacteristicAsync(Guid id)
    {
        var existing = await _context.Set<Characteristic>().FirstOrDefaultAsync(c => c.Id == id);
        if (existing == null) return false;
        _context.Set<Characteristic>().Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<Characteristic?> GetCharacteristicByIdAsync(Guid id)
    {
        return await _context.Set<Characteristic>().FirstOrDefaultAsync(c => c.Id == id);
    }
    public async Task<IEnumerable<Characteristic>> GetCharacteristicsByProductIdAsync(Guid productId)
    {
        return await _context.Set<Characteristic>()
            .Where(c => c.ProductId == productId)
            .ToListAsync();
    }
}

