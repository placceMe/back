using ProductsService.Models;
using ProductsService.Data;

namespace ProductsService.Repositories;

public class CharacteristicRepository : ICharacteristicRepository
{
    private readonly ProductsDBContext _context;
    public CharacteristicRepository(ProductsDBContext context)
    {
        _context = context;
    }
    public void CreateCharacteristic(Characteristic characteristic)
    {
        _context.Set<Characteristic>().Add(characteristic);
        _context.SaveChanges();
    }
    public bool UpdateCharacteristic(Guid id, Characteristic characteristic)
    {
        var existing = _context.Set<Characteristic>().FirstOrDefault(c => c.Id == id);
        if (existing == null) return false;
        existing.Value = characteristic.Value;
        existing.ProductId = characteristic.ProductId;
        existing.CharacteristicDictId = characteristic.CharacteristicDictId;
        _context.Set<Characteristic>().Update(existing);
        _context.SaveChanges();
        return true;
    }
    public bool DeleteCharacteristic(Guid id)
    {
        var existing = _context.Set<Characteristic>().FirstOrDefault(c => c.Id == id);
        if (existing == null) return false;
        _context.Set<Characteristic>().Remove(existing);
        _context.SaveChanges();
        return true;
    }
    public Characteristic? GetCharacteristicById(Guid id)
    {
        return _context.Set<Characteristic>().FirstOrDefault(c => c.Id == id);
    }
}
