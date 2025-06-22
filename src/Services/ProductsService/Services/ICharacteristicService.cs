using ProductsService.Models;


namespace ProductsService.Services;

public interface ICharacteristicService
{
    void CreateCharacteristic(Characteristic characteristic);
    bool UpdateCharacteristic(Guid id, Characteristic characteristic);
    bool DeleteCharacteristic(Guid id);
    Characteristic? GetCharacteristicById(Guid id);
}
