using ProductsService.Models;


namespace ProductsService.Services.Interfaces;

public interface ICharacteristicDictService
{
    Task CreateCharacteristicDictAsync(CharacteristicDict dict);
    Task<bool> UpdateCharacteristicDictAsync(Guid id, CharacteristicDict dict);
    Task<bool> DeleteCharacteristicDictAsync(Guid id);
    Task<CharacteristicDict?> GetCharacteristicDictByIdAsync(Guid id);
    Task<IEnumerable<CharacteristicDict>> GetCharacteristicDictsByCategoryIdAsync(Guid categoryId);
}
