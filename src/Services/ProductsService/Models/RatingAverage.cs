namespace ProductsService.Models;

public class RatingAverage
{
    public Guid ProductId { get; set; }
    public double AverageRating { get; set; }
    public uint TotalRatings { get; set; }
}
