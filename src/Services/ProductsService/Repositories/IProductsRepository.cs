using ProductsService.Models;

namespace ProductsService.Repositories;

public interface IProductsRepository
{
    // ...existing code...
    ProductEmbedding? GetProductEmbedding(Guid productId);
    void AddOrUpdateProductEmbedding(ProductEmbedding embedding);
    void CreateProduct(Product product);
    bool UpdateProduct(Guid id, Product product);
    bool DeleteProduct(Guid id);
    Product? GetProductById(Guid id);
    // ...existing code...
}