namespace FilesService.Services
{
    // Services/IImageService.cs
    public interface IFilesService
    {
        Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default);
        Task<Stream> GetImageAsync(string fileName, CancellationToken cancellationToken = default);
        Task<bool> DeleteImageAsync(string fileName, CancellationToken cancellationToken = default);
        Task<List<string>> GetAllImagesAsync(CancellationToken cancellationToken = default);
        Task<Stream?> GetObjectStreamAsync(string fileName, CancellationToken cancellationToken = default);
    }
}