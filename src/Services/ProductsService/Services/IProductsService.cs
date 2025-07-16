using ProductsService.Models;
using ProductsService.DTOs;

namespace ProductsService.Services;

public interface IProductsService
{
    ProductEmbedding? GetProductEmbedding(Guid productId);
    void AddOrUpdateProductEmbedding(ProductEmbedding embedding);
    void CreateProduct(Product product);
    Task<Product> CreateProductWithFilesAsync(CreateProductWithFilesDto createDto, CancellationToken cancellationToken = default);
    bool UpdateProduct(Guid id, Product product);
    Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
    bool DeleteProduct(Guid id);
    Product? GetProductById(Guid id);
    Task<Product?> GetProductByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Product>> GetProductsByIdsAsync(IEnumerable<Guid> ids);
    Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(Guid categoryId);
}