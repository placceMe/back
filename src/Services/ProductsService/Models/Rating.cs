namespace ProductsService.Models;

public class Rating
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public uint Value { get; set; } // Assuming Value is an integer rating
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string State { get; set; } = ProductState.Moderation;


}