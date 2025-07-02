using Microsoft.AspNetCore.Mvc;
using FilesService.Services;

namespace FilesService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFilesService _minioService;

        public FilesController(IFilesService imageService)
        {
            _minioService = imageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest("Only image files are allowed");

            var fileName = await _minioService.UploadImageAsync(file, cancellationToken);
            return Ok(new { FileName = fileName, Message = "File uploaded successfully" });
        }

        [HttpGet("file/{fileName}")]
        public async Task<IActionResult> GetImage(string fileName, CancellationToken cancellationToken)
        {
            try
            {
                var stream = await _minioService.GetImageAsync(fileName, cancellationToken);
                return File(stream, "application/octet-stream", fileName);
            }
            catch
            {
                return NotFound("Image not found");
            }
        }
        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetImage(string fileName)
        {
            var stream = await _minioService.GetObjectStreamAsync(fileName);
            if (stream == null)
                return NotFound();

            return File(stream, "image/jpeg"); // або визначай MIME динамічно
        }

        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteImage(string fileName, CancellationToken cancellationToken)
        {
            var result = await _minioService.DeleteImageAsync(fileName, cancellationToken);
            return result ? Ok("Image deleted successfully") : NotFound("Image not found");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllImages(CancellationToken cancellationToken)
        {
            var images = await _minioService.GetAllImagesAsync(cancellationToken);
            return Ok(images);
        }
    }
}