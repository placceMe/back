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
    }

    public async Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(file.OpenReadStream());
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);

        var response = await _httpClient.PostAsync("api/Files/upload", content, cancellationToken);
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
        var response = await _httpClient.DeleteAsync($"api/Files/{fileName}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<Stream> GetImageAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Files/{fileName}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    private class UploadResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}