namespace ProductsService.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public uint Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public uint Weight { get; set; }
    public Guid CategoryId { get; set; }
    public Guid SellerId { get; set; }
    public string State { get; set; } = string.Empty;
    public uint Quantity { get; set; }
    public CategoryDto? Category { get; set; }
}

public class CreateProductDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public uint Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public uint Weight { get; set; }
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
    public Guid CategoryId { get; set; }
    public uint Quantity { get; set; }
}