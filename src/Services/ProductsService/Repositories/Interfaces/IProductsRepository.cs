using Marketplace.Contracts.Products;
using Marketplace.Contracts.Files;
using Marketplace.Contracts.Common;
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
    Task<IEnumerable<Product>> GetProductsWithFilterAsync(int offset, int limit, Guid? sellerId = null, Guid? categoryId = null, string? status = null);
    Task<PaginationInfo> GetPaginationInfoWithFilterAsync(int offset, int limit, Guid? sellerId = null, Guid? categoryId = null, string? status = null);
}
