using ProductsService.Models;
using ProductsService.DTOs;

namespace ProductsService.Services.Interfaces;

public interface IAttachmentService
{
    Task<Attachment?> GetAttachmentByIdAsync(Guid id);
    Task<IEnumerable<Attachment>> GetAttachmentsByProductIdAsync(Guid productId);
    Task<Attachment> CreateAttachmentAsync(CreateAttachmentDto createDto, CancellationToken cancellationToken = default);
    Task<Attachment> UpdateAttachmentAsync(Attachment attachment);
    Task DeleteAttachmentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AttachmentExistsAsync(Guid id);
    Task<Stream> GetAttachmentFileAsync(Guid id, CancellationToken cancellationToken = default);
}