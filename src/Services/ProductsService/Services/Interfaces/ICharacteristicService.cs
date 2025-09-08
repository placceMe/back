using ProductsService.Models;
using Marketplace.Contracts.Products;
using Marketplace.Contracts.Files;
using Marketplace.Contracts.Common;

namespace ProductsService.Services.Interfaces;

public interface ICharacteristicService
{
    Task CreateCharacteristicAsync(Characteristic characteristic);
    Task<bool> UpdateCharacteristicAsync(UpdateCharacteristicDto characteristic);
    Task<bool> DeleteCharacteristicAsync(Guid id);
    Task<Characteristic?> GetCharacteristicByIdAsync(Guid id);
}

