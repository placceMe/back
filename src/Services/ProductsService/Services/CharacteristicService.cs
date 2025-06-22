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
    public void CreateCharacteristic(Characteristic characteristic) => _repository.CreateCharacteristic(characteristic);
    public bool UpdateCharacteristic(Guid id, Characteristic characteristic) => _repository.UpdateCharacteristic(id, characteristic);
    public bool DeleteCharacteristic(Guid id) => _repository.DeleteCharacteristic(id);
    public Characteristic? GetCharacteristicById(Guid id) => _repository.GetCharacteristicById(id);
}
