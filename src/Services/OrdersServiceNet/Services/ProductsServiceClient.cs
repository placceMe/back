using OrdersServiceNet.DTOs;

namespace OrdersServiceNet.Services;

public class ProductsServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsServiceClient> _logger;

    public ProductsServiceClient(HttpClient httpClient, ILogger<ProductsServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductInfo?> GetProductAsync(Guid productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/products/{productId}");
            if (response.IsSuccessStatusCode)
            {
                var product = await response.Content.ReadFromJsonAsync<ProductInfo>();
                return product;
            }
            else
            {
                _logger.LogWarning("Product {ProductId} not found. Status: {StatusCode}",
                    productId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId}", productId);
        }
        return null;
    }

    public async Task<Dictionary<Guid, ProductInfo>> GetProductsManyAsync(IEnumerable<Guid> productIds)
    {
        try
        {
            var idsDto = new IdsDto { Ids = productIds };
            var response = await _httpClient.PostAsJsonAsync("api/products/many", idsDto);

            if (response.IsSuccessStatusCode)
            {
                var products = await response.Content.ReadFromJsonAsync<List<ProductInfo>>();
                return products?.ToDictionary(p => p.Id, p => p) ?? new Dictionary<Guid, ProductInfo>();
            }
            else
            {
                _logger.LogWarning("Failed to fetch products. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching multiple products");
        }
        return new Dictionary<Guid, ProductInfo>();
    }

    public async Task<bool> ChangeProductQuantityAsync(Guid productId, string operation, int quantity)
    {
        try
        {
            var changeQuantityDto = new ChangeQuantityDto
            {
                Operation = operation,
                Quantity = quantity
            };

            var response = await _httpClient.PutAsJsonAsync($"api/products/{productId}/quantity", changeQuantityDto);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Product {ProductId} quantity updated. Operation: {Operation}, Quantity: {Quantity}",
                    productId, operation, quantity);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to update product {ProductId} quantity. Status: {StatusCode}",
                    productId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId} quantity", productId);
        }
        return false;
    }
}

public class ChangeQuantityDto
{
    public required string Operation { get; set; } // "add", "minus", "set"
    public required int Quantity { get; set; }
}