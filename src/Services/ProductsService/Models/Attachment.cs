namespace ProductsService.Models;

public class Attachment
{
    public Guid Id { get; set; }
    public string FilePath { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
}
