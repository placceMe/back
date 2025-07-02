using ProductsService.Models;

namespace ProductsService.Repositories;

public interface ICategoryRepository
{
    void CreateCategory(Category category);
    bool UpdateCategory(Guid id, Category category);
    bool DeleteCategory(Guid id);
    Category? GetCategoryById(Guid id);
    IEnumerable<Category> GetAllCategories();
}
