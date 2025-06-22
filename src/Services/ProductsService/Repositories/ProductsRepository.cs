using ProductsService.Models; // Змініть на відповідний простір імен
using ProductsService.Data;

namespace ProductsService.Repositories;

public class ProductsRepository : IProductsRepository
{
    private readonly ProductsDBContext _context;

    public ProductsRepository(ProductsDBContext context)
    {
        _context = context;
    }

    public ProductEmbedding? GetProductEmbedding(Guid productId)
    {
        return _context.ProductEmbeddings.FirstOrDefault(e => e.ProductId == productId);
    }

    public void AddOrUpdateProductEmbedding(ProductEmbedding embedding)
    {
        var existing = _context.ProductEmbeddings.FirstOrDefault(e => e.ProductId == embedding.ProductId);
        if (existing != null)
        {
            existing.Embedding = embedding.Embedding;
            _context.ProductEmbeddings.Update(existing);
        }
        else
        {
            _context.ProductEmbeddings.Add(embedding);
        }
        _context.SaveChanges();
    }

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
        existing.CategoryId = product.CategoryId;
        existing.SellerId = product.SellerId;
        existing.State = product.State;
        _context.Products.Update(existing);
        _context.SaveChanges();
        return true;
    }

    public bool DeleteProduct(Guid id)
    {
        var existing = _context.Products.FirstOrDefault(p => p.Id == id);
        if (existing == null) return false;
        existing.State = ProductState.Deleted;
        _context.Products.Update(existing);
        _context.SaveChanges();
        return true;
    }

    public Product? GetProductById(Guid id)
    {
        return _context.Products.FirstOrDefault(p => p.Id == id);
    }
}