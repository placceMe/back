using ProductsService.DTOs;
using ProductsService.Models;
using ProductsService.Extensions;
using ProductsService.Repositories.Interfaces;
using ProductsService.Services.Interfaces;

namespace ProductsService.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IProductsRepository _productRepository;

    public FeedbackService(IFeedbackRepository feedbackRepository, IProductsRepository productRepository)
    {
        _feedbackRepository = feedbackRepository;
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<FeedbackDto>> GetAllFeedbacksAsync()
    {
        var feedbacks = await _feedbackRepository.GetAllAsync();
        return feedbacks.Select(MapToDto);
    }

    public async Task<IEnumerable<FeedbackDto>> GetAllFeedbacksAsync(int offset, int limit)
    {
        var feedbacks = await _feedbackRepository.GetAllFeedbacksAsync(offset, limit);
        return feedbacks.Select(f => f.ToDto());
    }

    public async Task<FeedbackDto?> GetFeedbackByIdAsync(Guid id)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(id);
        return feedback != null ? MapToDto(feedback) : null;
    }

    public async Task<IEnumerable<FeedbackDto>> GetFeedbacksByProductIdAsync(Guid productId)
    {
        var feedbacks = await _feedbackRepository.GetByProductIdAsync(productId);
        return feedbacks.Select(MapToDto);
    }

    public async Task<IEnumerable<FeedbackDto>> GetFeedbacksByProductIdAsync(Guid productId, int offset, int limit)
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksByProductIdAsync(productId, offset, limit);
        return feedbacks.Select(f => f.ToDto());
    }

    public async Task<IEnumerable<FeedbackDto>> GetFeedbacksByUserIdAsync(Guid userId)
    {
        var feedbacks = await _feedbackRepository.GetByUserIdAsync(userId);
        return feedbacks.Select(MapToDto);
    }

    public async Task<IEnumerable<FeedbackDto>> GetFeedbacksByUserIdAsync(Guid userId, int offset, int limit)
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksByUserIdAsync(userId, offset, limit);
        return feedbacks.Select(f => f.ToDto());
    }

    public async Task<FeedbackDto> CreateFeedbackAsync(CreateFeedbackDto createFeedbackDto)
    {


        var feedback = new Feedback
        {
            Content = createFeedbackDto.Content,
            Rating = createFeedbackDto.Rating,
            ProductId = createFeedbackDto.ProductId,
            UserId = createFeedbackDto.UserId
        };

        var createdFeedback = await _feedbackRepository.CreateAsync(feedback);
        return MapToDto(createdFeedback);
    }

    public async Task<FeedbackDto> UpdateFeedbackAsync(Guid id, UpdateFeedbackDto updateFeedbackDto)
    {
        var existingFeedback = await _feedbackRepository.GetByIdAsync(id);
        if (existingFeedback == null)
        {
            throw new ArgumentException("Feedback not found");
        }

        existingFeedback.Content = updateFeedbackDto.Content;
        existingFeedback.Rating = updateFeedbackDto.Rating;

        var updatedFeedback = await _feedbackRepository.UpdateAsync(existingFeedback);
        return MapToDto(updatedFeedback);
    }

    public async Task<bool> DeleteFeedbackAsync(Guid id)
    {
        return await _feedbackRepository.DeleteAsync(id);
    }

    public async Task<FeedbackSummaryDto> GetFeedbackSummaryByProductIdAsync(Guid productId)
    {
        var averageRating = await _feedbackRepository.GetAverageRatingByProductIdAsync(productId);
        var totalFeedbacks = await _feedbackRepository.GetFeedbackCountByProductIdAsync(productId);
        var recentFeedbacks = await _feedbackRepository.GetByProductIdAsync(productId);

        return new FeedbackSummaryDto
        {
            ProductId = productId,
            AverageRating = averageRating,
            TotalFeedbacks = totalFeedbacks,
            RecentFeedbacks = recentFeedbacks.Take(5).Select(MapToDto).ToList()
        };
    }


    private static FeedbackDto MapToDto(Feedback feedback)
    {
        return new FeedbackDto
        {
            Id = feedback.Id,
            Content = feedback.Content,
            Rating = feedback.Rating,
            ProductId = feedback.ProductId,
            ProductName = feedback.Product?.Title,
            UserId = feedback.UserId,
            CreatedAt = DateTime.UtcNow // Note: Add CreatedAt to Feedback model later
        };
    }
}