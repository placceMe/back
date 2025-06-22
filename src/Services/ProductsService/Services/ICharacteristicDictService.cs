using ProductsService.Models;


namespace ProductsService.Services;

public interface ICharacteristicDictService
{
    void CreateCharacteristicDict(CharacteristicDict dict);
    bool UpdateCharacteristicDict(Guid id, CharacteristicDict dict);
    bool DeleteCharacteristicDict(Guid id);
    CharacteristicDict? GetCharacteristicDictById(Guid id);
}
