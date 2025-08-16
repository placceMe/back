using ProductsService.Models;

namespace ProductsService.Repositories.Interfaces;

public interface ICharacteristicRepository
{
    Task CreateCharacteristicAsync(Characteristic characteristic);
    Task<bool> UpdateCharacteristicAsync(Guid id, Characteristic characteristic);
    Task<bool> DeleteCharacteristicAsync(Guid id);
    Task<Characteristic?> GetCharacteristicByIdAsync(Guid id);
    Task<IEnumerable<Characteristic>> GetCharacteristicsByProductIdAsync(Guid productId);
}
