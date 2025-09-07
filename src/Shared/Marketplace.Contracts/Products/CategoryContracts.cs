using System.ComponentModel.DataAnnotations;

namespace Marketplace.Contracts.Products;

/// <summary>
/// Category data transfer object
/// </summary>
public class CategoryDto
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public Guid? ParentCategoryId { get; set; }
    
    [Url]
    public string ImageUrl { get; set; } = string.Empty;
    
    public List<CategoryDto> SubCategories { get; set; } = new();
}

/// <summary>
/// Create category request
/// </summary>
public class CreateCategoryDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public Guid? ParentCategoryId { get; set; }
    
    [Url]
    public string ImageUrl { get; set; } = string.Empty;
}

/// <summary>
/// Product characteristic DTO
/// </summary>
public class CharacteristicDto
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Value { get; set; } = string.Empty;
    
    public Guid CharacteristicDictId { get; set; }
    public CharacteristicDictDto? CharacteristicDict { get; set; }
}

/// <summary>
/// Create characteristic request
/// </summary>
public class CreateCharacteristicDto
{
    [Required]
    [StringLength(200)]
    public string Value { get; set; } = string.Empty;
    
    [Required]
    public Guid CharacteristicDictId { get; set; }
}

/// <summary>
/// Update characteristic request
/// </summary>
public class UpdateCharacteristicDto
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Characteristic dictionary DTO
/// </summary>
public class CharacteristicDictDto
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;
}