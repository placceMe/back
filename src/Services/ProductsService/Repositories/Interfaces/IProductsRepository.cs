using ProductsService.DTOs;
using ProductsService.Models;

namespace ProductsService.Repositories.Interfaces;

public interface IProductsRepository
{
    // Existing ProductEmbedding methods
    ProductEmbedding GetProductEmbedding(Guid productId);
    void AddOrUpdateProductEmbedding(ProductEmbedding embedding);

    // Product CRUD methods
    void CreateProduct(Product product);
    bool UpdateProduct(Guid id, Product product);
    bool DeleteProduct(Guid id);
    Product? GetProductById(Guid id);
    IEnumerable<Product> GetAllProducts();
    IEnumerable<Product> GetProductsByCategoryId(Guid categoryId);
    IEnumerable<Product> GetProductsBySellerId(Guid sellerId);

    // Async methods for pagination
    Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(Guid categoryId);
    Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(Guid categoryId, int offset, int limit);
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Product>> GetAllProductsAsync(int offset, int limit);
    Task<PaginationInfo> GetPaginationInfoAsync(int offset, int limit, Guid? categoryId);
    Task<IEnumerable<Product>> SearchProductsByTitleAsync(string query);
    Task<IEnumerable<Product>> GetProductsBySellerIdAsync(Guid sellerId, int offset, int limit);
    Task<int> GetProductsCountBySellerIdAsync(Guid sellerId);
    Task<Product?> GetProductByIdAsync(Guid id);
    Task UpdateProductAsync(Product product);
    Task<IEnumerable<Product>> GetProductsByStateAsync(string state);
}