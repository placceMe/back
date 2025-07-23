using ProductsService.Models;
using ProductsService.Repositories;


namespace ProductsService.Services;

public class CharacteristicDictService : ICharacteristicDictService
{
    private readonly ICharacteristicDictRepository _repository;
    public CharacteristicDictService(ICharacteristicDictRepository repository)
    {
        _repository = repository;
    }
    public async Task CreateCharacteristicDictAsync(CharacteristicDict dict) => await _repository.CreateCharacteristicDictAsync(dict);
    public async Task<bool> UpdateCharacteristicDictAsync(Guid id, CharacteristicDict dict) => await _repository.UpdateCharacteristicDictAsync(id, dict);
    public async Task<bool> DeleteCharacteristicDictAsync(Guid id) => await _repository.DeleteCharacteristicDictAsync(id);
    public async Task<CharacteristicDict?> GetCharacteristicDictByIdAsync(Guid id) => await _repository.GetCharacteristicDictByIdAsync(id);
    public async Task<IEnumerable<CharacteristicDict>> GetCharacteristicDictsByCategoryIdAsync(Guid categoryId) => await _repository.GetCharacteristicDictsByCategoryIdAsync(categoryId);
}
