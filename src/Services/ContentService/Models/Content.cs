using System.ComponentModel.DataAnnotations;

namespace ContentService.Models;

public class Content
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string ContentType { get; set; } = "text"; // text, html, json, markdown

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = "general"; // general, homepage, about, contact, etc.

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
}
