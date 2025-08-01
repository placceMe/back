using System.ComponentModel.DataAnnotations;

namespace ProductsService.DTOs;

public class FeedbackDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public uint Rating { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateFeedbackDto
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public uint Rating { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid UserId { get; set; }
}

public class UpdateFeedbackDto
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public uint Rating { get; set; }
}

public class FeedbackSummaryDto
{
    public Guid ProductId { get; set; }
    public double AverageRating { get; set; }
    public int TotalFeedbacks { get; set; }
    public List<FeedbackDto> RecentFeedbacks { get; set; } = new List<FeedbackDto>();
}