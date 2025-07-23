namespace ProductsService.Models;

public class Characteristic
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public Guid CharacteristicDictId { get; set; }

}