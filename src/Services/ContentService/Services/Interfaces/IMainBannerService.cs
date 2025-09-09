using ContentService.DTOs;
using ContentService.Models;

namespace ContentService.Services.Interfaces;

public interface IMainBannerService
{
    Task<IEnumerable<MainBannerDto>> GetAllAsync();
    Task<IEnumerable<MainBannerDto>> GetVisibleAsync();
    Task<MainBannerDto?> GetByIdAsync(int id);
    Task<MainBannerDto> CreateAsync(CreateMainBannerDto createDto, CancellationToken cancellationToken = default);
    Task<MainBannerDto?> UpdateAsync(int id, UpdateMainBannerDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
