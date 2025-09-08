using Marketplace.Contracts.Orders;
using Marketplace.Contracts.Users;
using Marketplace.Contracts.Common;
using System.Text.Json;

namespace OrdersServiceNet.Services;

public class UsersServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersServiceClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public UsersServiceClient(HttpClient httpClient, ILogger<UsersServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} exists", userId);
            return false;
        }
    }

    public async Task<UserInfo?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting user info for {UserId}", userId);
            var response = await _httpClient.GetAsync($"api/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("User {UserId} not found. Status: {StatusCode}", userId, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserInfo>(json, _jsonOptions);

            _logger.LogInformation("Successfully retrieved user info for {UserId}", userId);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return null;
        }
    }
}
