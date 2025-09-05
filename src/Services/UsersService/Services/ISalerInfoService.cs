using UsersService.Models;

namespace UsersService.Services;

public interface ISalerInfoService
{
    Task<IEnumerable<SalerInfo>> GetAllAsync();
    Task<SalerInfo?> GetByIdAsync(Guid id);
    Task<IEnumerable<SalerInfo>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<SalerInfo?> GetByUserIdAsync(Guid userId);
    Task CreateAsync(SalerInfo salerInfo);
    Task<bool> UpdateAsync(SalerInfo salerInfo);
    Task<bool> DeleteAsync(Guid id);
}