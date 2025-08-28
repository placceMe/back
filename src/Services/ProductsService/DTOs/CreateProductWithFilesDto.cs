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
    public List<CreateCharacteristicDto> Characteristics { get; set; } = new List<CreateCharacteristicDto>();
    public Guid CategoryId { get; set; }
    public Guid SellerId { get; set; }
    public uint Quantity { get; set; }
    public string Producer { get; set; } = string.Empty;
    public bool IsNew { get; set; }
}