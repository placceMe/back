using ProductsService.Models;
using ProductsService.DTOs;

namespace ProductsService.Repositories.Interfaces;

public interface ICharacteristicRepository
{
    Task CreateCharacteristicAsync(Characteristic characteristic);
    Task<bool> UpdateCharacteristicAsync(UpdateCharacteristicDto characteristic);
    Task<bool> DeleteCharacteristicAsync(Guid id);
    Task<Characteristic?> GetCharacteristicByIdAsync(Guid id);
    Task<IEnumerable<Characteristic>> GetCharacteristicsByProductIdAsync(Guid productId);
}
