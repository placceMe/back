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
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}

/// <summary>
/// Create category request
/// </summary>
/// </summary>
public class CreateCategoryDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

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
    public string Name { get; set; } = string.Empty;
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

/// <summary>
/// Update category request
/// </summary>
public class UpdateCategoryDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Category state request
/// </summary>
public class CategoryStateDto
{
    [Required]
    [StringLength(50)]
    public string State { get; set; } = string.Empty;
}

/// <summary>
/// Category with full information
/// </summary>
public class CategoryFullInfo : CategoryDto
{
    public int ProductsCount { get; set; }
    public int CharacteristicsCount { get; set; }
}

/// <summary>
/// Delete category request
/// </summary>
public class DeleteCategoryDto
{
    [Required]
    public Guid CategoryId { get; set; }

    public Guid? ReplacementCategoryId { get; set; }
}

/// <summary>
/// Base characteristic DTO
/// </summary>
public class BaseCharacteristicDto
{
    [Required]
    [StringLength(500)]
    public string Value { get; set; } = string.Empty;
}