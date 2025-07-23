namespace ProductsService.DTOs;

public class CharacteristicDto : CreateCharacteristicDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;

}

public class CreateCharacteristicDto : BaseCharacteristicDto
{
    public Guid CharacteristicDictId { get; set; }

}

public class BaseCharacteristicDto
{
    public string Value { get; set; } = string.Empty;

}