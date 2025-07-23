namespace ProductsService.DTOs;

public class CharacteristicDto
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public Guid CharacteristicDictId { get; set; }
}

public class CreateCharacteristicDto
{
    public string Value { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public Guid CharacteristicDictId { get; set; }
}