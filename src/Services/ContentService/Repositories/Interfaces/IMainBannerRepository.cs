using ContentService.Models;

namespace ContentService.Repositories.Interfaces;

public interface IMainBannerRepository
{
    Task<IEnumerable<MainBanner>> GetAllAsync();
    Task<IEnumerable<MainBanner>> GetVisibleAsync();
    Task<MainBanner?> GetByIdAsync(int id);
    Task<MainBanner> CreateAsync(MainBanner mainBanner);
    Task<MainBanner> UpdateAsync(MainBanner mainBanner);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
