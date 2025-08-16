using ProductsService.Models;

namespace ProductsService.Repositories.Interfaces;

public interface IFeedbackRepository
{
    Task<IEnumerable<Feedback>> GetAllAsync();
    Task<Feedback?> GetByIdAsync(Guid id);
    Task<IEnumerable<Feedback>> GetByProductIdAsync(Guid productId);
    Task<IEnumerable<Feedback>> GetByUserIdAsync(Guid userId);
    Task<Feedback> CreateAsync(Feedback feedback);
    Task<Feedback> UpdateAsync(Feedback feedback);
    Task<bool> DeleteAsync(Guid id);
    Task<double> GetAverageRatingByProductIdAsync(Guid productId);
    Task<int> GetFeedbackCountByProductIdAsync(Guid productId);
    Task<IEnumerable<Feedback>> GetAllFeedbacksAsync();
    Task<IEnumerable<Feedback>> GetAllFeedbacksAsync(int offset, int limit);
    Task<IEnumerable<Feedback>> GetFeedbacksByProductIdAsync(Guid productId);
    Task<IEnumerable<Feedback>> GetFeedbacksByProductIdAsync(Guid productId, int offset, int limit);
    Task<IEnumerable<Feedback>> GetFeedbacksByUserIdAsync(Guid userId);
    Task<IEnumerable<Feedback>> GetFeedbacksByUserIdAsync(Guid userId, int offset, int limit);
}