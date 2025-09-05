using UsersService.Models;
using UsersService.Repositories;

namespace UsersService.Services;

public class SalerInfoService : ISalerInfoService
{
    private readonly ISalerInfoRepository _repository;
    private readonly ILogger<SalerInfoService> _logger;

    public SalerInfoService(ISalerInfoRepository repository, ILogger<SalerInfoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<IEnumerable<SalerInfo>> GetAllAsync() => _repository.GetAllAsync();

    public Task<SalerInfo?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    public Task<IEnumerable<SalerInfo>> GetByIdsAsync(IEnumerable<Guid> ids) => _repository.GetByIdsAsync(ids);

    public Task<SalerInfo?> GetByUserIdAsync(Guid userId) => _repository.GetByUserIdAsync(userId);

    public async Task CreateAsync(SalerInfo salerInfo)
    {
        _logger.LogInformation("Creating SalerInfo for user {UserId}", salerInfo.UserId);
        await _repository.AddAsync(salerInfo);
    }

    public async Task<bool> UpdateAsync(SalerInfo salerInfo)
    {
        _logger.LogInformation("Updating SalerInfo {SalerInfoId}", salerInfo.Id);
        return await _repository.UpdateAsync(salerInfo);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting SalerInfo {SalerInfoId}", id);
        return await _repository.DeleteAsync(id);
    }
}