namespace ProductsService.Models;

public class Attachment
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public required Product Product { get; set; }
}
