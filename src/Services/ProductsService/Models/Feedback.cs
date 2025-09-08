using System.ComponentModel.DataAnnotations;

namespace ProductsService.Models;

public class Feedback
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public uint RatingService { get; set; }
    public uint RatingSpeed { get; set; }
    public uint RatingDescription { get; set; }
    public uint RatingAvailable { get; set; }
    public uint RatingAverage { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = FeedbackStatus.New;

}

public static class FeedbackStatus
{
    public static readonly string New = "New";
    public static readonly string Approved = "Approved";
    public static readonly string Rejected = "Rejected";
}
