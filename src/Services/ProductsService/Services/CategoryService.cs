using ProductsService.Models;
using ProductsService.Repositories.Interfaces;
using ProductsService.Services.Interfaces;


namespace ProductsService.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }
    public void CreateCategory(Category category) => _repository.CreateCategory(category);
    public bool UpdateCategory(Guid id, Category category) => _repository.UpdateCategory(id, category);
    public bool DeleteCategory(Guid id) => _repository.DeleteCategory(id);
    public Category? GetCategoryById(Guid id) => _repository.GetCategoryById(id);

    public IEnumerable<Category> GetAllCategories()
    {
        return _repository.GetAllCategories().ToList();
    }
    public async Task<bool> UpdateCategoryState(Guid id, DTOs.CategoryStateDto stateDto)
    {
        var category = _repository.GetCategoryById(id);
        if (category == null) return false;

        category.Status = stateDto.State;
        return _repository.UpdateCategory(id, category);
    }

    public async Task<bool> DeleteCategoryWithTransfer(Guid id, DTOs.DeleteCategoryDto deleteDto)
    {
        var category = _repository.GetCategoryById(id);
        if (category == null) return false;

        // If transfer category is specified, transfer all products first
        if (deleteDto.TransferToCategoryId.HasValue)
        {
            var targetCategory = _repository.GetCategoryById(deleteDto.TransferToCategoryId.Value);
            if (targetCategory == null) return false;

            await _repository.TransferProductsToCategoryAsync(id, deleteDto.TransferToCategoryId.Value);
        }

        // Change category status to Deleted
        category.Status = CategoryState.Deleted;
        return _repository.UpdateCategory(id, category);
    }
}
