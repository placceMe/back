using System.ComponentModel.DataAnnotations;

namespace ContentService.DTOs;

public class CreateContentDto
{
    [Required]
    [MaxLength(200)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string ContentType { get; set; } = "text";

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = "general";

    public bool IsActive { get; set; } = true;
}

public class UpdateContentDto
{
    [Required]
    public string Value { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string ContentType { get; set; } = "text";

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = "general";

    public bool IsActive { get; set; } = true;
}

public class ContentDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

// FAQ DTOs
public class CreateFaqDto
{
    [Required]
    [MaxLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;

    public int Order { get; set; } = 0;
}

public class UpdateFaqDto
{
    [Required]
    [MaxLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;

    public int Order { get; set; } = 0;
}

public class FaqDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}
