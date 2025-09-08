using ProductsService.Models;

namespace ProductsService.Repositories.Interfaces;

public interface IAttachmentRepository
{
    Task<Attachment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Attachment>> GetByProductIdAsync(Guid productId);
    Task<IEnumerable<Attachment>> GetAllAsync();
    Task<Attachment> CreateAsync(Attachment attachment);
    Task<Attachment> UpdateAsync(Attachment attachment);
    Task DeleteAsync(Guid id);
}
