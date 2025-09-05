using ChatService.DTOs;

namespace ChatService.Services
{
    public class UsersServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UsersServiceClient> _logger;

        public UsersServiceClient(HttpClient httpClient, ILogger<UsersServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserInfo?> GetUserInfoAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();
                    return userInfo;
                }
                else
                {
                    _logger.LogWarning("Failed to get user info for user {UserId}. Status: {StatusCode}", 
                        userId, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info for user {UserId}", userId);
                return null;
            }
        }

        public async Task<List<UserInfo>> GetUsersInfoAsync(IEnumerable<Guid> userIds)
        {
            try
            {
                var idsParam = string.Join(",", userIds);
                var response = await _httpClient.GetAsync($"/api/users/batch?ids={idsParam}");
                
                if (response.IsSuccessStatusCode)
                {
                    var usersInfo = await response.Content.ReadFromJsonAsync<List<UserInfo>>();
                    return usersInfo ?? new List<UserInfo>();
                }
                else
                {
                    _logger.LogWarning("Failed to get users info. Status: {StatusCode}", response.StatusCode);
                    return new List<UserInfo>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users info");
                return new List<UserInfo>();
            }
        }
    }

    public class UserInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}