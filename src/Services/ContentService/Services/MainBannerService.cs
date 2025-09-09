using ContentService.DTOs;
using ContentService.Models;
using ContentService.Repositories;
using ContentService.Services.Interfaces;

namespace ContentService.Services;

public class MainBannerService : IMainBannerService
{
    private readonly IMainBannerRepository _repository;
    private readonly IFilesServiceClient _filesServiceClient;
    private readonly ILogger<MainBannerService> _logger;

    public MainBannerService(
        IMainBannerRepository repository,
        IFilesServiceClient filesServiceClient,
        ILogger<MainBannerService> logger)
    {
        _repository = repository;
        _filesServiceClient = filesServiceClient;
        _logger = logger;
    }

    public async Task<IEnumerable<MainBannerDto>> GetAllAsync()
    {
        var banners = await _repository.GetAllAsync();
        return banners.Select(ToDto);
    }

    public async Task<IEnumerable<MainBannerDto>> GetVisibleAsync()
    {
        var banners = await _repository.GetVisibleAsync();
        return banners.Select(ToDto);
    }

    public async Task<MainBannerDto?> GetByIdAsync(int id)
    {
        var banner = await _repository.GetByIdAsync(id);
        return banner != null ? ToDto(banner) : null;
    }

    public async Task<MainBannerDto> CreateAsync(CreateMainBannerDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Upload image to MinIO via FilesService
            var imageFileName = await _filesServiceClient.UploadImageAsync(createDto.Image, cancellationToken);

            var mainBanner = new MainBanner
            {
                ImageUrl = imageFileName,
                IsVisible = createDto.IsVisible,
                Order = createDto.Order,
                CreatedBy = "system", // TODO: Get from authentication context
                UpdatedBy = "system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdBanner = await _repository.CreateAsync(mainBanner);
            _logger.LogInformation("MainBanner created with ID {BannerId}", createdBanner.Id);

            return ToDto(createdBanner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create MainBanner");
            throw;
        }
    }

    public async Task<MainBannerDto?> UpdateAsync(int id, UpdateMainBannerDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingBanner = await _repository.GetByIdAsync(id);
            if (existingBanner == null)
                return null;

            string? oldImageUrl = null;

            // If new image is provided, upload it and get the filename
            if (updateDto.Image != null)
            {
                oldImageUrl = existingBanner.ImageUrl; // Store for deletion later
                existingBanner.ImageUrl = await _filesServiceClient.UploadImageAsync(updateDto.Image, cancellationToken);
            }

            // Update other properties if provided
            if (updateDto.IsVisible.HasValue)
                existingBanner.IsVisible = updateDto.IsVisible.Value;

            if (updateDto.Order.HasValue)
                existingBanner.Order = updateDto.Order.Value;

            existingBanner.UpdatedBy = "system"; // TODO: Get from authentication context
            existingBanner.UpdatedAt = DateTime.UtcNow;

            var updatedBanner = await _repository.UpdateAsync(existingBanner);

            // Delete old image if a new one was uploaded
            if (!string.IsNullOrEmpty(oldImageUrl) && updateDto.Image != null)
            {
                try
                {
                    await _filesServiceClient.DeleteImageAsync(oldImageUrl, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old image {ImageUrl} for MainBanner {BannerId}", oldImageUrl, id);
                }
            }

            _logger.LogInformation("MainBanner {BannerId} updated successfully", id);
            return ToDto(updatedBanner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update MainBanner {BannerId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var banner = await _repository.GetByIdAsync(id);
            if (banner == null)
                return false;

            // Delete the image from MinIO
            if (!string.IsNullOrEmpty(banner.ImageUrl))
            {
                try
                {
                    await _filesServiceClient.DeleteImageAsync(banner.ImageUrl, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete image {ImageUrl} for MainBanner {BannerId}", banner.ImageUrl, id);
                }
            }

            var result = await _repository.DeleteAsync(id);

            if (result)
                _logger.LogInformation("MainBanner {BannerId} deleted successfully", id);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete MainBanner {BannerId}", id);
            throw;
        }
    }

    private static MainBannerDto ToDto(MainBanner banner)
    {
        return new MainBannerDto
        {
            Id = banner.Id,
            ImageUrl = banner.ImageUrl,
            IsVisible = banner.IsVisible,
            Order = banner.Order,
            CreatedAt = banner.CreatedAt,
            UpdatedAt = banner.UpdatedAt,
            CreatedBy = banner.CreatedBy,
            UpdatedBy = banner.UpdatedBy
        };
    }
}
