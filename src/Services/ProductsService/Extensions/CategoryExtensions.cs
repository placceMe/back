using ProductsService.Models;
using ProductsService.DTOs;

namespace ProductsService.Extensions;

public static class CategoryExtensions
{
    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Status = category.Status
        };
    }

    public static IEnumerable<CategoryDto> ToDto(this IEnumerable<Category> categories)
    {
        return categories.Select(c => c.ToDto());
    }
}

public static class FeedbackExtensions
{
    public static FeedbackDto ToDto(this Feedback feedback)
    {
        return new FeedbackDto
        {
            Id = feedback.Id,
            ProductId = feedback.ProductId,
            UserId = feedback.UserId,
            Rating = feedback.Rating,
            CreatedAt = feedback.CreatedAt
        };
    }

    public static IEnumerable<FeedbackDto> ToDto(this IEnumerable<Feedback> feedbacks)
    {
        return feedbacks.Select(f => f.ToDto());
    }
}