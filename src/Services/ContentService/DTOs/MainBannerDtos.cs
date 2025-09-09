using System.ComponentModel.DataAnnotations;

namespace ContentService.DTOs;

public class CreateMainBannerDto
{
    [Required]
    public IFormFile Image { get; set; } = null!;

    public bool IsVisible { get; set; } = true;

    public int Order { get; set; } = 0;
}

public class UpdateMainBannerDto
{
    public IFormFile? Image { get; set; }

    public bool? IsVisible { get; set; }

    public int? Order { get; set; }
}

public class MainBannerDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}
