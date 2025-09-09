using ContentService.Models;

namespace ContentService.Repositories;

public interface IContentRepository
{
    Task<IEnumerable<Content>> GetAllAsync();
    Task<Content?> GetByIdAsync(int id);
    Task<Content?> GetByKeyAsync(string key);
    Task<IEnumerable<Content>> GetByCategoryAsync(string category);
    Task<IEnumerable<Content>> GetActiveContentAsync();
    Task<Content> CreateAsync(Content content);
    Task<Content> UpdateAsync(Content content);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(string key);
}
