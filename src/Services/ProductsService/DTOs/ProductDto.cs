namespace ProductsService.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public uint Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public uint Weight { get; set; }
    public string MainImageUrl { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid SellerId { get; set; }
    public string State { get; set; } = string.Empty;
    public uint Quantity { get; set; }
    public CategoryDto? Category { get; set; }
    public List<AttachmentDto> AdditionalImageUrls { get; set; } = new List<AttachmentDto>();
    public List<CharacteristicDto> Characteristics { get; set; } = new List<CharacteristicDto>();
}

public class ProductsDto
{
    public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    public PaginationInfo Pagination { get; set; } = new PaginationInfo();
}

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class CreateProductDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public uint Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public uint Weight { get; set; }
    public string MainImageUrl { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid SellerId { get; set; }
    public uint Quantity { get; set; }
}

public class UpdateProductDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public uint Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public uint Weight { get; set; }
    public string MainImageUrl { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public uint Quantity { get; set; }
    public List<UpdateCharacteristicDto> Characteristics { get; set; } = new List<UpdateCharacteristicDto>();
}

public class SearchProductsDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public uint Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public string MainImageUrl { get; set; } = string.Empty;
    public Guid Id { get; set; }

}

public class CreateProductCharacteristicDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
public class UpdateCharacteristicDto
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
}