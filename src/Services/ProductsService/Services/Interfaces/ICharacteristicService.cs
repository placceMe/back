using ProductsService.Models;


namespace ProductsService.Services.Interfaces;

public interface ICharacteristicService
{
    Task CreateCharacteristicAsync(Characteristic characteristic);
    Task<bool> UpdateCharacteristicAsync(Guid id, Characteristic characteristic);
    Task<bool> DeleteCharacteristicAsync(Guid id);
    Task<Characteristic?> GetCharacteristicByIdAsync(Guid id);
}
