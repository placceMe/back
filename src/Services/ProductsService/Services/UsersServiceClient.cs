using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Json;

namespace ProductsService.Services
{
    public class UsersServiceClient
    {
        private readonly HttpClient _httpClient;

        public UsersServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Example: Get user by ID
        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}");
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
    }

    // Example DTO, replace with shared contract if available
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
