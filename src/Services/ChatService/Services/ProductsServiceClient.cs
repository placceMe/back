using Marketplace.Contracts.Chat;
using Marketplace.Contracts.Common;
using Marketplace.Contracts.Products;

namespace ChatService.Services
{
    public class ProductsServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductsServiceClient> _logger;

        public ProductsServiceClient(HttpClient httpClient, ILogger<ProductsServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// ??????? ??????? ?? ID ?? ?????????, ?? sellerId ?????????? ???????? ??????
        /// </summary>
        public async Task<ProductValidationResult> ValidateProductSellerAsync(Guid productId, Guid sellerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/products/{productId}/validate-seller/{sellerId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ProductValidationResultDto>();

                    if (result == null)
                    {
                        return new ProductValidationResult { IsValid = false, Error = "Invalid response" };
                    }

                    return new ProductValidationResult
                    {
                        IsValid = result.IsValid,
                        Error = result.Error,
                        Product = result.Product != null ? new ProductInfo
                        {
                            Id = result.Product.Id,
                            Title = result.Product.Title,
                            Description = result.Product.Description,
                            Price = result.Product.Price,
                            MainImageUrl = result.Product.MainImageUrl,
                            SellerId = result.Product.SellerId,
                            CreatedAt = result.Product.CreatedAt
                        } : null
                    };
                }
                else
                {
                    _logger.LogWarning("Product seller validation failed for product {ProductId} and seller {SellerId}. Status: {StatusCode}",
                        productId, sellerId, response.StatusCode);

                    return new ProductValidationResult
                    {
                        IsValid = false,
                        Error = $"Validation request failed with status {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating product seller for product {ProductId} and seller {SellerId}",
                    productId, sellerId);

                return new ProductValidationResult
                {
                    IsValid = false,
                    Error = "Service error during validation"
                };
            }
        }

        public async Task<ProductInfo?> GetProductInfoAsync(Guid productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/products/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    var productInfo = await response.Content.ReadFromJsonAsync<ProductInfo>();
                    return productInfo;
                }
                else
                {
                    _logger.LogWarning("Failed to get product info for product {ProductId}. Status: {StatusCode}",
                        productId, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product info for product {ProductId}", productId);
                return null;
            }
        }
    }

    public class ProductValidationResult
    {
        public bool IsValid { get; set; }
        public string? Error { get; set; }
        public ProductInfo? Product { get; set; }
    }

    public class ProductInfo
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? MainImageUrl { get; set; }
        public Guid SellerId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
