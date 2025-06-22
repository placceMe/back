using ProductsService.Models;

namespace ProductsService.Repositories;

public interface ICharacteristicDictRepository
{
    void CreateCharacteristicDict(CharacteristicDict dict);
    bool UpdateCharacteristicDict(Guid id, CharacteristicDict dict);
    bool DeleteCharacteristicDict(Guid id);
    CharacteristicDict? GetCharacteristicDictById(Guid id);
}
