using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Options;
using FilesService.Models;
using Minio.DataModel;

namespace FilesService.Services;

public class FilesService : IFilesService
{
    private readonly IMinioClient _minioClient;
    private readonly MinIOConfig _config;
    private readonly ILogger<FilesService> _logger;

    public FilesService(IMinioClient minioClient, IOptions<MinIOConfig> config, ILogger<FilesService> logger)
    {
        _minioClient = minioClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            // Перевірка існування bucket
            await EnsureBucketExistsAsync(cancellationToken);

            // Генерація унікального імені файлу
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            // Завантаження файлу
            using var stream = file.OpenReadStream();
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(file.Length)
                .WithContentType(file.ContentType);

            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            _logger.LogInformation($"File {fileName} uploaded successfully");
            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            throw;
        }
    }

    public async Task<Stream> GetImageAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var memoryStream = new MemoryStream();
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(fileName)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));

            await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting file {fileName}");
            throw;
        }
    }
    public async Task<Stream?> GetObjectStreamAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();

        try
        {
            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(fileName)
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(memoryStream);
                }));

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return null;
        }
    }


    public async Task<bool> DeleteImageAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(fileName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);
            _logger.LogInformation($"File {fileName} deleted successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file {fileName}");
            return false;
        }
    }

    public async Task<List<string>> GetAllImagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var images = new List<string>();
            var listObjectsArgs = new ListObjectsArgs()
                .WithBucket(_config.BucketName)
                .WithRecursive(true);

            await foreach (var item in _minioClient.ListObjectsEnumAsync(listObjectsArgs, cancellationToken))
            {
                images.Add(item.Key);
            }

            return images;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files");
            throw;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken = default)
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(_config.BucketName);
        var found = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);

        if (!found)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(_config.BucketName);
            await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
            _logger.LogInformation($"Bucket {_config.BucketName} created");
        }
    }
}