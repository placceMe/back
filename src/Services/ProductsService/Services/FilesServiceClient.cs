using ProductsService.Services.Interfaces;
using System.Text.Json;

namespace ProductsService.Services;

public class FilesServiceClient : IFilesServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FilesServiceClient> _logger;

    public FilesServiceClient(HttpClient httpClient, ILogger<FilesServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Log the BaseAddress for debugging
        _logger.LogInformation($"FilesServiceClient initialized with BaseAddress: {_httpClient.BaseAddress}");
    }

    public async Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(file.OpenReadStream());
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);

        // Check if BaseAddress is set, if not use absolute URL
        string requestUri;
        if (_httpClient.BaseAddress != null)
        {
            requestUri = "api/files/upload";
            _logger.LogInformation($"Using relative URL: {requestUri} with BaseAddress: {_httpClient.BaseAddress}");
        }
        else
        {
            requestUri = "http://files-service:80/api/files/upload";
            _logger.LogWarning($"BaseAddress not set, using absolute URL: {requestUri}");
        }

        var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<UploadResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.FileName ?? throw new InvalidOperationException("Failed to get filename from upload response");
    }

    public async Task<bool> DeleteImageAsync(string fileName, CancellationToken cancellationToken = default)
    {
        string requestUri = _httpClient.BaseAddress != null
            ? $"api/files/{fileName}"
            : $"http://files-service:80/api/files/{fileName}";

        var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<Stream> GetImageAsync(string fileName, CancellationToken cancellationToken = default)
    {
        string requestUri = _httpClient.BaseAddress != null
            ? $"api/Files/{fileName}"
            : $"http://files-service:80/api/Files/{fileName}";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    private class UploadResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
