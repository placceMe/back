using ProductsService.Models;
using ProductsService.Repositories;


namespace ProductsService.Services;

public class CharacteristicService : ICharacteristicService
{
    private readonly ICharacteristicRepository _repository;
    public CharacteristicService(ICharacteristicRepository repository)
    {
        _repository = repository;
    }
    public async Task CreateCharacteristicAsync(Characteristic characteristic) => await _repository.CreateCharacteristicAsync(characteristic);
    public async Task<bool> UpdateCharacteristicAsync(Guid id, Characteristic characteristic) => await _repository.UpdateCharacteristicAsync(id, characteristic);
    public async Task<bool> DeleteCharacteristicAsync(Guid id) => await _repository.DeleteCharacteristicAsync(id);
    public async Task<Characteristic?> GetCharacteristicByIdAsync(Guid id) => await _repository.GetCharacteristicByIdAsync(id);
}
