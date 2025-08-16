using UsersService.Models;

namespace UsersService.Repositories;

public interface ISalerInfoRepository
{
    Task<IEnumerable<SalerInfo>> GetAllAsync();
    Task<SalerInfo?> GetByIdAsync(Guid id);
    Task<SalerInfo?> GetByUserIdAsync(Guid userId);
    Task AddAsync(SalerInfo salerInfo);
    Task<bool> UpdateAsync(SalerInfo salerInfo);
    Task<bool> DeleteAsync(Guid id);
}