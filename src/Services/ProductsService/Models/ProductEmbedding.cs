namespace ProductsService.Models;

public class ProductEmbedding
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public required float[] Embedding { get; set; }

}
