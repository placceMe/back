namespace ProductsService.Services.Interfaces;

public interface IFilesServiceClient
{
    Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(string fileName, CancellationToken cancellationToken = default);
    Task<Stream> GetImageAsync(string fileName, CancellationToken cancellationToken = default);
}
