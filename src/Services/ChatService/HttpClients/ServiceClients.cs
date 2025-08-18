using ChatService.DTOs;

namespace ChatService.HttpClients;

public interface IUsersServiceClient
{
    Task<UserDto?> GetUserByIdAsync(Guid userId);
}

public interface IProductsServiceClient
{
    Task<ProductDto?> GetProductByIdAsync(Guid productId);
}

public class UsersServiceClient : IUsersServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersServiceClient> _logger;

    public UsersServiceClient(HttpClient httpClient, ILogger<UsersServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                return user;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UsersService for user {UserId}", userId);
        }

        return null;
    }
}

public class ProductsServiceClient : IProductsServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsServiceClient> _logger;

    public ProductsServiceClient(HttpClient httpClient, ILogger<ProductsServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/products/{productId}");
            if (response.IsSuccessStatusCode)
            {
                var product = await response.Content.ReadFromJsonAsync<ProductDto>();
                return product;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling ProductsService for product {ProductId}", productId);
        }

        return null;
    }
}