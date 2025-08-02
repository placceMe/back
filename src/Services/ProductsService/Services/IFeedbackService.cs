using ProductsService.DTOs;

namespace ProductsService.Services;

public interface IFeedbackService
{
    Task<IEnumerable<FeedbackDto>> GetAllFeedbacksAsync();
    Task<IEnumerable<FeedbackDto>> GetAllFeedbacksAsync(int offset, int limit);
    Task<FeedbackDto?> GetFeedbackByIdAsync(Guid id);
    Task<IEnumerable<FeedbackDto>> GetFeedbacksByProductIdAsync(Guid productId);
    Task<IEnumerable<FeedbackDto>> GetFeedbacksByProductIdAsync(Guid productId, int offset, int limit);
    Task<IEnumerable<FeedbackDto>> GetFeedbacksByUserIdAsync(Guid userId);
    Task<IEnumerable<FeedbackDto>> GetFeedbacksByUserIdAsync(Guid userId, int offset, int limit);
    Task<FeedbackDto> CreateFeedbackAsync(CreateFeedbackDto createFeedbackDto);
    Task<FeedbackDto> UpdateFeedbackAsync(Guid id, UpdateFeedbackDto updateFeedbackDto);
    Task<bool> DeleteFeedbackAsync(Guid id);
    Task<FeedbackSummaryDto> GetFeedbackSummaryByProductIdAsync(Guid productId);
}