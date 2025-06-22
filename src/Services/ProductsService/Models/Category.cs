namespace ProductsService.Models;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string Status { get; set; } = CategoryState.Active;
    public ICollection<Product> Products { get; set; }
}
