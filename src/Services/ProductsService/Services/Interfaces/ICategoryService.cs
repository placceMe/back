using ProductsService.Models;
using ProductsService.DTOs;

namespace ProductsService.Services.Interfaces;

public interface ICategoryService
{
    void CreateCategory(Category category);
    bool UpdateCategory(Guid id, Category category);
    bool DeleteCategory(Guid id);
    Category? GetCategoryById(Guid id);
    IEnumerable<Category> GetAllCategories();
    Task<bool> UpdateCategoryState(Guid id, CategoryStateDto stateDto);
    Task<bool> DeleteCategoryWithTransfer(Guid id, DeleteCategoryDto deleteDto);
}
