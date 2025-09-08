using System.ComponentModel.DataAnnotations;

namespace Marketplace.Contracts.Products;

/// <summary>
/// Feedback data transfer object
/// </summary>
public class FeedbackDto
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public uint RatingService { get; set; }

    [Required]
    [Range(1, 5)]
    public uint RatingSpeed { get; set; }

    [Required]
    [Range(1, 5)]
    public uint RatingDescription { get; set; }

    [Required]
    [Range(1, 5)]
    public uint RatingAvailable { get; set; }

    public uint RatingAverage { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [StringLength(200)]
    public string? ProductName { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public FeedbackUserDto User { get; set; } = new();

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "New";
}

/// <summary>
/// Feedback user information
/// </summary>
public class FeedbackUserDto
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Surname { get; set; } = string.Empty;
}

/// <summary>
/// Create feedback request
/// </summary>
public class CreateFeedbackDto
{
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public uint RatingService { get; set; }

    [Required]
    [Range(1, 5)]
    public uint RatingSpeed { get; set; }

    [Required]
    [Range(1, 5)]
    public uint RatingDescription { get; set; }

    [Required]
    [Range(1, 5)]
    public uint RatingAvailable { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid UserId { get; set; }
}

/// <summary>
/// Update feedback request
/// </summary>
public class UpdateFeedbackDto
{
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public uint RatingService { get; set; }

    [Required]
    [Range(1, 5)]
    public uint RatingSpeed { get; set; }

    [Required]
    [Range(1, 5)]
    public uint RatingDescription { get; set; }

    [Required]
    [Range(1, 5)]
    public uint RatingAvailable { get; set; }
}

/// <summary>
/// Update feedback status request
/// </summary>
public class UpdateFeedbackStatusDto
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Feedback summary
/// </summary>
public class FeedbackSummaryDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(0, 5)]
    public double AverageRating { get; set; }

    [Range(0, int.MaxValue)]
    public int TotalFeedbacks { get; set; }

    public List<FeedbackDto> RecentFeedbacks { get; set; } = new();
}
