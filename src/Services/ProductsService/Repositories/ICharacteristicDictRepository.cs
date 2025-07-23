using ProductsService.Models;

namespace ProductsService.Repositories;

public interface ICharacteristicDictRepository
{
    Task CreateCharacteristicDictAsync(CharacteristicDict dict);
    Task<bool> UpdateCharacteristicDictAsync(Guid id, CharacteristicDict dict);
    Task<bool> DeleteCharacteristicDictAsync(Guid id);
    Task<CharacteristicDict?> GetCharacteristicDictByIdAsync(Guid id);
    Task<IEnumerable<CharacteristicDict>> GetCharacteristicDictsByCategoryIdAsync(Guid categoryId);
    Task<IEnumerable<CharacteristicDict>> GetAllCharacteristicDictsAsync();
}
