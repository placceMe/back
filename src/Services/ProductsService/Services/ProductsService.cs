using ProductsService.Models;
using ProductsService.Repositories;


namespace ProductsService.Services;

public class ProductsService : IProductsService
{
    private readonly IProductsRepository _repository;

    public ProductsService(IProductsRepository repository)
    {
        _repository = repository;
    }

    public void CreateProduct(Product product)
    {
        _repository.CreateProduct(product);
        // Створюємо embedding з дефолтним вектором (наприклад, 0-і)
        var embedding = new ProductEmbedding
        {
            ProductId = product.Id,
            Embedding = new float[128] // або інша довжина за замовчуванням
        };
        _repository.AddOrUpdateProductEmbedding(embedding);
    }

    public bool UpdateProduct(Guid id, Product product)
    {
        return _repository.UpdateProduct(id, product);
    }

    public bool DeleteProduct(Guid id)
    {
        return _repository.DeleteProduct(id);
    }

    public Product? GetProductById(Guid id)
    {
        return _repository.GetProductById(id);
    }

    public ProductEmbedding? GetProductEmbedding(Guid productId)
    {
        return _repository.GetProductEmbedding(productId);
    }

    public void AddOrUpdateProductEmbedding(ProductEmbedding embedding)
    {
        _repository.AddOrUpdateProductEmbedding(embedding);
    }

    // ...інші методи...
}