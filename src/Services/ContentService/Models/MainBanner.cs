using System.ComponentModel.DataAnnotations;

namespace ContentService.Models;

public class MainBanner
{
    public int Id { get; set; }

    [Required]
    [MaxLength(1000)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;

    public int Order { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
}
