using ProductsService.Models;
using Marketplace.Contracts.Products;
using Marketplace.Contracts.Files;
using Marketplace.Contracts.Common;
using ProductsService.Repositories.Interfaces;
using ProductsService.Services.Interfaces;

namespace ProductsService.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IFilesServiceClient _filesServiceClient;
    private readonly ILogger<AttachmentService> _logger;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        IFilesServiceClient filesServiceClient,
        ILogger<AttachmentService> logger)
    {
        _attachmentRepository = attachmentRepository;
        _filesServiceClient = filesServiceClient;
        _logger = logger;
    }

    public async Task<Attachment?> GetAttachmentByIdAsync(Guid id)
    {
        return await _attachmentRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Attachment>> GetAttachmentsByProductIdAsync(Guid productId)
    {
        return await _attachmentRepository.GetByProductIdAsync(productId);
    }

    public async Task<Attachment> CreateAttachmentAsync(CreateAttachmentDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Завантажуємо файл у FilesService
            var fileName = await _filesServiceClient.UploadImageAsync(createDto.File, cancellationToken);

            // Створюємо запис у базі даних
            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                FilePath = fileName,
                ProductId = createDto.ProductId,
                Product = null! // Буде завантажено через EF
            };

            return await _attachmentRepository.CreateAsync(attachment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create attachment for product {ProductId}", createDto.ProductId);
            throw;
        }
    }

    public async Task<Attachment> UpdateAttachmentAsync(Attachment attachment)
    {
        return await _attachmentRepository.UpdateAsync(attachment);
    }

    public async Task DeleteAttachmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(id);
        if (attachment != null)
        {
            // Видаляємо файл з FilesService
            await _filesServiceClient.DeleteImageAsync(attachment.FilePath, cancellationToken);

            // Видаляємо запис з бази даних
            await _attachmentRepository.DeleteAsync(id);
        }
    }

    public async Task<bool> AttachmentExistsAsync(Guid id)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(id);
        return attachment != null;
    }

    public async Task<Stream> GetAttachmentFileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(id);
        if (attachment == null)
            throw new FileNotFoundException($"Attachment with id {id} not found");

        return await _filesServiceClient.GetImageAsync(attachment.FilePath, cancellationToken);
    }
}
