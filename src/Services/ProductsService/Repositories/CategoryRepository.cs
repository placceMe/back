using ProductsService.Models;
using ProductsService.Data;
using ProductsService.Repositories.Interfaces;
using Marketplace.Contracts.Products;
using Marketplace.Contracts.Files;
using Marketplace.Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace ProductsService.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ProductsDBContext _context;
    public CategoryRepository(ProductsDBContext context)
    {
        _context = context;
    }
    public void CreateCategory(Category category)
    {
        _context.Categories.Add(category);
        _context.SaveChanges();
    }
    public bool UpdateCategory(Guid id, Category category)
    {
        var existing = _context.Categories.FirstOrDefault(c => c.Id == id);
        if (existing == null) return false;
        existing.Name = category.Name;
        existing.Status = category.Status;
        _context.Categories.Update(existing);
        _context.SaveChanges();
        return true;
    }
    public bool DeleteCategory(Guid id)
    {
        var existing = _context.Categories.FirstOrDefault(c => c.Id == id);
        if (existing == null) return false;
        existing.Status = CategoryState.Deleted;
        _context.Categories.Update(existing);
        _context.SaveChanges();
        return true;
    }
    public Category? GetCategoryById(Guid id)
    {
        return _context.Categories.FirstOrDefault(c => c.Id == id);
    }
    public IEnumerable<Category> GetAllCategories()
    {
        return _context.Categories.Where(c => c.Status != CategoryState.Deleted).ToList();
    }

    public async Task TransferProductsToCategoryAsync(Guid fromCategoryId, Guid toCategoryId)
    {
        var products = _context.Products.Where(p => p.CategoryId == fromCategoryId).ToList();
        foreach (var product in products)
        {
            product.CategoryId = toCategoryId;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CategoryFullInfo>> GetAllCategoriesFullInfo()
    {
        var categories = await _context.Categories.ToListAsync();
        var result = new List<CategoryFullInfo>();
        foreach (var category in categories)
        {
            var productsCount = await _context.Products.CountAsync(p => p.CategoryId == category.Id);
            var characteristicsCount = await _context.CharacteristicDicts.CountAsync(c => c.CategoryId == category.Id);
            result.Add(new CategoryFullInfo
            {
                Id = category.Id,
                Name = category.Name,
                Status = category.Status,
                CreatedAt = category.CreatedAt,
                ProductsCount = productsCount,
                CharacteristicsCount = characteristicsCount
            });
        }
        return result;
    }
}

