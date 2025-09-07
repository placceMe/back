using ProductsService.Models;
using ProductsService.DTOs;

namespace ProductsService.Services.Interfaces;

public interface IProductsService
{
    ProductEmbedding? GetProductEmbedding(Guid productId);
    void AddOrUpdateProductEmbedding(ProductEmbedding embedding);
    void CreateProduct(Product product);
    Task<Product> CreateProductWithFilesAsync(CreateProductWithFilesDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductAsync(Guid id, Product product, IEnumerable<UpdateCharacteristicDto>? characteristics = null);
    Task<Product?> UpdateProductWithFilesAsync(Guid id, UpdateProductWithFilesDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
    bool DeleteProduct(Guid id);
    Product? GetProductById(Guid id);
    Task<Product?> GetProductByIdAsync(Guid id);
    Task<ProductsDto> GetAllProductsAsync(int offset, int limit);
    Task<IEnumerable<Product>> GetProductsByIdsAsync(IEnumerable<Guid> ids);
    Task<ProductsDto> GetProductsByCategoryIdAsync(Guid categoryId, int offset, int limit);
    Task<ProductsDto> GetProductsBySellerIdAsync(Guid sellerId, int offset, int limit);
    Task<IEnumerable<SearchProductsDto>> SearchProductsByTitleAsync(string query);
    Task<bool> ChangeProductStateAsync(Guid id, string state);
    Task<bool> ChangeProductQuantityAsync(Guid id, string operation, int quantity);
    Task<IEnumerable<Product>> GetProductsByStateAsync(string state);
}