using ProductsService.Models;
using ProductsService.Repositories;


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
}
