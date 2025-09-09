using System.ComponentModel.DataAnnotations;

namespace ContentService.Models;

public class Faq
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;

    public int Order { get; set; } = 0; // Для сортування

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
}
