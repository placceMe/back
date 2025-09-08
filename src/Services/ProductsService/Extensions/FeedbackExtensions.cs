using Marketplace.Contracts.Products;
using Marketplace.Contracts.Files;
using Marketplace.Contracts.Common;
using ProductsService.Models;

namespace ProductsService.Extensions;

public static class FeedbackExtensions
{
    public static FeedbackDto ToDto(this Feedback feedback)
    {
        return new FeedbackDto
        {
            Id = feedback.Id,
            Content = feedback.Content,
            RatingService = feedback.RatingService,
            RatingSpeed = feedback.RatingSpeed,
            RatingDescription = feedback.RatingDescription,
            RatingAvailable = feedback.RatingAvailable,
            RatingAverage = feedback.RatingAverage,
            ProductId = feedback.ProductId,
            ProductName = feedback.Product?.Title,
            UserId = feedback.UserId,
            CreatedAt = feedback.CreatedAt
        };
    }
}
