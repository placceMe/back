using ProductsService.Models;

namespace ProductsService.Repositories;

public interface ICharacteristicRepository
{
    void CreateCharacteristic(Characteristic characteristic);
    bool UpdateCharacteristic(Guid id, Characteristic characteristic);
    bool DeleteCharacteristic(Guid id);
    Characteristic? GetCharacteristicById(Guid id);
}
