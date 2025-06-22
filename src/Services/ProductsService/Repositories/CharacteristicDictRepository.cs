using ProductsService.Models;
using ProductsService.Data;

namespace ProductsService.Repositories;

public class CharacteristicDictRepository : ICharacteristicDictRepository
{
    private readonly ProductsDBContext _context;
    public CharacteristicDictRepository(ProductsDBContext context)
    {
        _context = context;
    }
    public void CreateCharacteristicDict(CharacteristicDict dict)
    {
        _context.Set<CharacteristicDict>().Add(dict);
        _context.SaveChanges();
    }
    public bool UpdateCharacteristicDict(Guid id, CharacteristicDict dict)
    {
        var existing = _context.Set<CharacteristicDict>().FirstOrDefault(d => d.Id == id);
        if (existing == null) return false;
        existing.Name = dict.Name;
        existing.CategoryId = dict.CategoryId;
        _context.Set<CharacteristicDict>().Update(existing);
        _context.SaveChanges();
        return true;
    }
    public bool DeleteCharacteristicDict(Guid id)
    {
        var existing = _context.Set<CharacteristicDict>().FirstOrDefault(d => d.Id == id);
        if (existing == null) return false;
        _context.Set<CharacteristicDict>().Remove(existing);
        _context.SaveChanges();
        return true;
    }
    public CharacteristicDict? GetCharacteristicDictById(Guid id)
    {
        return _context.Set<CharacteristicDict>().FirstOrDefault(d => d.Id == id);
    }
}
