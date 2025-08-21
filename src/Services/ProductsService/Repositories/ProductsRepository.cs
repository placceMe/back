using ProductsService.Models;
using ProductsService.Data;
using Microsoft.EntityFrameworkCore;
using ProductsService.Repositories.Interfaces;
using ProductsService.DTOs;


namespace ProductsService.Repositories;

public class ProductsRepository : IProductsRepository
{
    private readonly ProductsDBContext _context;

    public ProductsRepository(ProductsDBContext context)
    {
        _context = context;
    }

    // ProductEmbeddings functionality is currently disabled - commenting out references
    // The ProductEmbeddings DbSet is commented out in ProductsDBContext

    // Comment out or remove any references to _context.ProductEmbeddings until the feature is implemented

    // public ProductEmbedding? GetProductEmbedding(Guid productId)
    // {
    //     return _context.ProductEmbeddings.FirstOrDefault(e => e.ProductId == productId);
    // }

    // public void AddOrUpdateProductEmbedding(ProductEmbedding embedding)
    // {
    //     var existing = _context.ProductEmbeddings.FirstOrDefault(e => e.ProductId == embedding.ProductId);
    //     if (existing != null)
    //     {
    //         existing.Embedding = embedding.Embedding;
    //         _context.ProductEmbeddings.Update(existing);
    //     }
    //     else
    //     {
    //         _context.ProductEmbeddings.Add(embedding);
    //     }
    //     _context.SaveChanges();
    // }

    // IProductRepository implementation
    public void CreateProduct(Product product)
    {
        _context.Products.Add(product);
        _context.SaveChanges();
    }

    public bool UpdateProduct(Guid id, Product product)
    {
        var existing = _context.Products.FirstOrDefault(p => p.Id == id);
        if (existing == null) return false;

        existing.Title = product.Title;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Color = product.Color;
        existing.Weight = product.Weight;
        existing.MainImageUrl = product.MainImageUrl;
        existing.CategoryId = product.CategoryId;
        existing.Quantity = product.Quantity;

        _context.Products.Update(existing);
        _context.SaveChanges();
        return true;
    }

    public bool DeleteProduct(Guid id)
    {
        var existing = _context.Products.FirstOrDefault(p => p.Id == id);
        if (existing == null) return false;

        _context.Products.Remove(existing);
        _context.SaveChanges();
        return true;
    }

    public Product? GetProductById(Guid id)
    {
        return _context.Products
            .Include(p => p.Category)
            .Include(p => p.Characteristics)
            .ThenInclude(c => c.CharacteristicDict)
            .Include(p => p.Attachments)
            .FirstOrDefault(p => p.Id == id);
    }

    public IEnumerable<Product> GetAllProducts()
    {
        return _context.Products
            .Include(p => p.Category)
            .ToList();
    }

    public IEnumerable<Product> GetProductsByCategoryId(Guid categoryId)
    {
        return _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .ToList();
    }

    public IEnumerable<Product> GetProductsBySellerId(Guid sellerId)
    {
        return _context.Products
            .Include(p => p.Category)
            .Where(p => p.SellerId == sellerId)
            .ToList();
    }

    // Temporary implementations for disabled ProductEmbeddings functionality
    public ProductEmbedding GetProductEmbedding(Guid productId)
    {
        // ProductEmbeddings functionality is currently disabled
        // Return empty embedding as placeholder
        return new ProductEmbedding { ProductId = productId, Embedding = new float[0] };
    }

    public void AddOrUpdateProductEmbedding(ProductEmbedding productEmbedding)
    {
        // ProductEmbeddings functionality is currently disabled
        // TODO: Implement when ProductEmbeddings DbSet is enabled
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(Guid categoryId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(Guid categoryId, int offset, int limit)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync(int offset, int limit)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<PaginationInfo> GetPaginationInfoAsync(int offset, int limit, Guid? categoryId)
    {
        IQueryable<Product> query = _context.Products;

        if (categoryId != null)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        int totalItems = await query.CountAsync();

        var paginationInfo = new PaginationInfo
        {
            TotalItems = totalItems,
            PageSize = limit,
            CurrentPage = (int)Math.Ceiling((double)offset / limit) + 1
        };
        return paginationInfo;
    }

    public async Task<IEnumerable<Product>> SearchProductsByTitleAsync(string query)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Title.ToLower().Contains(query.ToLower()))
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsBySellerIdAsync(Guid sellerId, int offset, int limit)
    {
        return await _context.Products
            .Where(p => p.SellerId == sellerId)
            .OrderBy(p => p.Title)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetProductsCountBySellerIdAsync(Guid sellerId)
    {
        return await _context.Products.CountAsync(p => p.SellerId == sellerId);
    }
}