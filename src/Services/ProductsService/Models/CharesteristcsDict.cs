namespace ProductsService.Models;

public class CharacteristicDict
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }

}