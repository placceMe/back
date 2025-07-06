namespace ProductsService.DTOs;

public class CreateAttachmentDto
{
    public IFormFile File { get; set; } = null!;
    public Guid ProductId { get; set; }
}