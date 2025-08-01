using ProductsService.Models;

namespace ProductsService.Repositories;

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
}