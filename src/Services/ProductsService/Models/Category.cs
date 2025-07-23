namespace ProductsService.Models;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = CategoryState.Active;
    public List<Product> Products { get; set; } = new List<Product>();
    public List<CharacteristicDict> Characteristics { get; set; } = new List<CharacteristicDict>();
}
