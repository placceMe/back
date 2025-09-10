using Marketplace.Contracts.Orders;
using Marketplace.Contracts.Products;
using Marketplace.Contracts.Common;

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

    public async Task<Dictionary<Guid, Guid>> GetProductSellerMappingAsync(IEnumerable<Guid> productIds)
    {
        try
        {
            var mapping = new Dictionary<Guid, Guid>();
            var productIdsList = productIds.ToList();

            // Process in batches to avoid too many concurrent requests
            const int batchSize = 10;
            for (int i = 0; i < productIdsList.Count; i += batchSize)
            {
                var batch = productIdsList.Skip(i).Take(batchSize);
                var batchTasks = batch.Select(async productId =>
                {
                    try
                    {
                        var response = await _httpClient.GetAsync($"api/products/{productId}");
                        if (response.IsSuccessStatusCode)
                        {
                            var productData = await response.Content.ReadAsStringAsync();
                            var productJson = System.Text.Json.JsonDocument.Parse(productData);

                            if (productJson.RootElement.TryGetProperty("sellerId", out var sellerIdElement) &&
                                sellerIdElement.TryGetGuid(out var sellerId))
                            {
                                return new KeyValuePair<Guid, Guid>(productId, sellerId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Error fetching seller info for product {ProductId}: {Error}",
                            productId, ex.Message);
                    }
                    return (KeyValuePair<Guid, Guid>?)null;
                });

                var batchResults = await Task.WhenAll(batchTasks);
                foreach (var result in batchResults)
                {
                    if (result.HasValue)
                    {
                        mapping[result.Value.Key] = result.Value.Value;
                    }
                }
            }

            return mapping;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product seller mapping");
        }
        return new Dictionary<Guid, Guid>();
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
