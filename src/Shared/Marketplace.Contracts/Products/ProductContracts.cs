using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Marketplace.Contracts.Common;
using Marketplace.Contracts.Files;

namespace Marketplace.Contracts.Products;

/// <summary>
/// Product data transfer object
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string Producer { get; set; } = string.Empty;

    public bool IsNew { get; set; } = false;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(1, uint.MaxValue, ErrorMessage = "Price must be positive")]
    public uint Price { get; set; }

    [StringLength(50)]
    public string Color { get; set; } = string.Empty;

    public uint Weight { get; set; }

    [Url]
    public string MainImageUrl { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public Guid SellerId { get; set; }

    [StringLength(50)]
    public string State { get; set; } = string.Empty;

    public uint Quantity { get; set; }

    public CategoryDto? Category { get; set; }
    public List<AttachmentDto> AdditionalImageUrls { get; set; } = new();
    public List<CharacteristicDto> Characteristics { get; set; } = new();
}

/// <summary>
/// Products collection response
/// </summary>
public class ProductsDto : PagedResponse<ProductDto>
{
    public List<ProductDto> Products
    {
        get => Data.ToList();
        set => Data = value;
    }
}

/// <summary>
/// Create product request
/// </summary>
public class CreateProductDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string Producer { get; set; } = string.Empty;

    public bool IsNew { get; set; } = false;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, uint.MaxValue)]
    public uint Price { get; set; }

    [StringLength(50)]
    public string Color { get; set; } = string.Empty;

    public uint Weight { get; set; }

    [Url]
    public string MainImageUrl { get; set; } = string.Empty;

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public Guid SellerId { get; set; }

    [Range(0, uint.MaxValue)]
    public uint Quantity { get; set; }
}

/// <summary>
/// Update product request
/// </summary>
public class UpdateProductDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string Producer { get; set; } = string.Empty;

    public bool IsNew { get; set; } = false;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, uint.MaxValue)]
    public uint Price { get; set; }

    [StringLength(50)]
    public string Color { get; set; } = string.Empty;

    public uint Weight { get; set; }

    [Url]
    public string MainImageUrl { get; set; } = string.Empty;

    [Required]
    public Guid CategoryId { get; set; }

    [Range(0, uint.MaxValue)]
    public uint Quantity { get; set; }

    public List<UpdateCharacteristicDto> Characteristics { get; set; } = new();
}

/// <summary>
/// Create product with files (for web interface)
/// </summary>
public class CreateProductWithFilesDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string Producer { get; set; } = string.Empty;

    public bool IsNew { get; set; } = false;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, uint.MaxValue)]
    public uint Price { get; set; }

    [StringLength(50)]
    public string Color { get; set; } = string.Empty;

    public uint Weight { get; set; }

    public IFormFile? MainImage { get; set; }

    public List<IFormFile> AdditionalImages { get; set; } = new();

    public List<CreateCharacteristicDto> Characteristics { get; set; } = new();

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public Guid SellerId { get; set; }

    [Range(0, uint.MaxValue)]
    public uint Quantity { get; set; }
}

/// <summary>
/// Update product with files (for web interface)
/// </summary>
public class UpdateProductWithFilesDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string Producer { get; set; } = string.Empty;

    public bool IsNew { get; set; } = false;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, uint.MaxValue)]
    public uint Price { get; set; }

    [StringLength(50)]
    public string Color { get; set; } = string.Empty;

    public uint Weight { get; set; }

    public IFormFile? MainImage { get; set; }

    public List<IFormFile> AdditionalImages { get; set; } = new();

    public List<UpdateCharacteristicDto> Characteristics { get; set; } = new();

    [Required]
    public Guid CategoryId { get; set; }

    [Range(0, uint.MaxValue)]
    public uint Quantity { get; set; }

    /// <summary>
    /// IDs of existing images to delete
    /// </summary>
    public List<Guid> ImagesToDelete { get; set; } = new();
}

/// <summary>
/// Product search result
/// </summary>
public class SearchProductsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public uint Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public string MainImageUrl { get; set; } = string.Empty;
}

/// <summary>
/// Change product state request
/// </summary>
public class ChangeProductStateDto
{
    [Required]
    [StringLength(50)]
    public string State { get; set; } = string.Empty;
}

/// <summary>
/// Change product quantity request
/// </summary>
public class ChangeQuantityDto
{
    [Required]
    [RegularExpression("^(add|minus|set)$", ErrorMessage = "Operation must be 'add', 'minus', or 'set'")]
    public string Operation { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}

/// <summary>
/// Product filter parameters
/// </summary>
public class ProductFilterDto
{
    public Guid? SellerId { get; set; }
    public Guid? CategoryId { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }
}

/// <summary>
/// IDs collection DTO
/// </summary>
public class IdsDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one ID is required")]
    public IEnumerable<Guid> Ids { get; set; } = new List<Guid>();
}

/// <summary>
/// Create product characteristic request
/// </summary>
public class CreateProductCharacteristicDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Product seller validation result
/// </summary>
public class ProductValidationResultDto
{
    public bool IsValid { get; set; }
    public string? Error { get; set; }
    public ProductInfoDto? Product { get; set; }
}

/// <summary>
/// Product info for validation
/// </summary>
public class ProductInfoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? MainImageUrl { get; set; }
    public Guid SellerId { get; set; }
    public DateTime CreatedAt { get; set; }
}