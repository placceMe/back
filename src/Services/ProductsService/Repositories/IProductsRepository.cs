using ProductsService.Models;

namespace ProductsService.Repositories;

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
}