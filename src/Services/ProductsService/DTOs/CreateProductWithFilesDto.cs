namespace ProductsService.DTOs;

public class CreateProductWithFilesDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public uint Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public uint Weight { get; set; }
    public IFormFile? MainImage { get; set; }
    public List<IFormFile> AdditionalImages { get; set; } = new List<IFormFile>();
    public Guid CategoryId { get; set; }
    public Guid SellerId { get; set; }
    public uint Quantity { get; set; }
}