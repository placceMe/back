using Microsoft.EntityFrameworkCore;
using ProductsService.Data;
using ProductsService.Models;
using ProductsService.Repositories.Interfaces;


namespace ProductsService.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly ProductsDBContext _context;

    public FeedbackRepository(ProductsDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Feedback>> GetAllAsync()
    {
        return await _context.Feedbacks
            .Include(f => f.Product)
            .ToListAsync();
    }

    public async Task<Feedback?> GetByIdAsync(Guid id)
    {
        return await _context.Feedbacks
            .Include(f => f.Product)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<Feedback>> GetByProductIdAsync(Guid productId)
    {
        return await _context.Feedbacks
            .Where(f => f.ProductId == productId)
            .OrderByDescending(f => f.Id) // Assuming you want newest first
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Feedbacks
            .Include(f => f.Product)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.Id)
            .ToListAsync();
    }

    public async Task<Feedback> CreateAsync(Feedback feedback)
    {
        feedback.Id = Guid.NewGuid();
        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();
        return feedback;
    }

    public async Task<Feedback> UpdateAsync(Feedback feedback)
    {
        _context.Feedbacks.Update(feedback);
        await _context.SaveChangesAsync();
        return feedback;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var feedback = await _context.Feedbacks.FindAsync(id);
        if (feedback == null)
            return false;

        _context.Feedbacks.Remove(feedback);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Feedbacks.AnyAsync(f => f.Id == id);
    }

    public async Task<double> GetAverageRatingByProductIdAsync(Guid productId)
    {
        var feedbacks = await _context.Feedbacks
            .Where(f => f.ProductId == productId)
            .ToListAsync();

        if (!feedbacks.Any())
            return 0;

        return feedbacks.Average(f => f.RatingAverage);
    }

    public async Task<int> GetFeedbackCountByProductIdAsync(Guid productId)
    {
        return await _context.Feedbacks
            .CountAsync(f => f.ProductId == productId);
    }

    public async Task<IEnumerable<Feedback>> GetAllFeedbacksAsync()
    {
        return await _context.Feedbacks
            .Include(f => f.Product)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetAllFeedbacksAsync(int offset, int limit)
    {
        return await _context.Feedbacks
            .Include(f => f.Product)
            .OrderByDescending(f => f.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetFeedbacksByProductIdAsync(Guid productId)
    {
        return await _context.Feedbacks
            .Include(f => f.Product)
            .Where(f => f.ProductId == productId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetFeedbacksByProductIdAsync(Guid productId, int offset, int limit)
    {
        return await _context.Feedbacks
            .Include(f => f.Product)
            .Where(f => f.ProductId == productId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetFeedbacksByUserIdAsync(Guid userId)
    {
        return await _context.Feedbacks
            .Include(f => f.Product)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetFeedbacksByUserIdAsync(Guid userId, int offset, int limit)
    {
        return await _context.Feedbacks
            .Include(f => f.Product)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetFeedbacksBySellerIdAsync(Guid sellerId, int offset, int limit)
    {
        return await _context.Feedbacks
            .Include(f => f.Product)
            .Where(f => f.Product != null && f.Product.SellerId == sellerId)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetFeedbacksByStatusAsync(string status)
    {
        return await _context.Feedbacks
            .Include(f => f.Product)
            .Where(f => f.Status == status)
            .ToListAsync();
    }
}