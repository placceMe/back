using System.ComponentModel.DataAnnotations;
using ProductsService.Models;

namespace ProductsService.DTOs;

public class FeedbackDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public uint RatingService { get; set; }
    public uint RatingSpeed { get; set; }
    public uint RatingDescription { get; set; }
    public uint RatingAvailable { get; set; }
    public uint RatingAverage { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public FeedbackUserDto User { get; set; } = new();
    public string Status { get; set; } = FeedbackStatus.New;
}

public class FeedbackUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
}

public class CreateFeedbackDto
{
    public string Content { get; set; } = string.Empty;
    public uint RatingService { get; set; }
    public uint RatingSpeed { get; set; }
    public uint RatingDescription { get; set; }
    public uint RatingAvailable { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
}

public class UpdateFeedbackDto
{
    public string Content { get; set; } = string.Empty;
    public uint RatingService { get; set; }
    public uint RatingSpeed { get; set; }
    public uint RatingDescription { get; set; }
    public uint RatingAvailable { get; set; }
}

public class FeedbackSummaryDto
{
    public Guid ProductId { get; set; }
    public double AverageRating { get; set; }
    public int TotalFeedbacks { get; set; }
    public List<FeedbackDto> RecentFeedbacks { get; set; } = new();
}