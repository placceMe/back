using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Marketplace.Contracts.Files;

/// <summary>
/// File upload response
/// </summary>
public class FileUploadResponseDto
{
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

/// <summary>
/// Bulk file upload response
/// </summary>
public class BulkFileUploadResponseDto
{
    public List<FileUploadResponseDto> Files { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Attachment DTO
/// </summary>
public class AttachmentDto
{
    public Guid Id { get; set; }
    
    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    public long Size { get; set; }
    
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;
    
    public Guid ProductId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Create attachment request
/// </summary>
public class CreateAttachmentDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
    
    [Required]
    public Guid ProductId { get; set; }
}