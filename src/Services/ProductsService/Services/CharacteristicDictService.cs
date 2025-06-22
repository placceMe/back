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
    public void CreateCharacteristicDict(CharacteristicDict dict) => _repository.CreateCharacteristicDict(dict);
    public bool UpdateCharacteristicDict(Guid id, CharacteristicDict dict) => _repository.UpdateCharacteristicDict(id, dict);
    public bool DeleteCharacteristicDict(Guid id) => _repository.DeleteCharacteristicDict(id);
    public CharacteristicDict? GetCharacteristicDictById(Guid id) => _repository.GetCharacteristicDictById(id);
}
