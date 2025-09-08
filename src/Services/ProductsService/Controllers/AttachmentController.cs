using Microsoft.AspNetCore.Mvc;
using ProductsService.Models;
using Marketplace.Contracts.Products;
using Marketplace.Contracts.Files;
using Marketplace.Contracts.Common;
using ProductsService.Services.Interfaces;

namespace ProductsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttachmentController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Attachment>> GetAttachment(Guid id)
    {
        var attachment = await _attachmentService.GetAttachmentByIdAsync(id);
        if (attachment == null)
        {
            return NotFound();
        }
        return attachment;
    }

    [HttpGet("{id}/file")]
    public async Task<IActionResult> GetAttachmentFile(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var stream = await _attachmentService.GetAttachmentFileAsync(id, cancellationToken);
            return File(stream, "application/octet-stream");
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<Attachment>>> GetAttachmentsByProduct(Guid productId)
    {
        var attachments = await _attachmentService.GetAttachmentsByProductIdAsync(productId);
        return Ok(attachments);
    }

    [HttpPost]
    public async Task<ActionResult<Attachment>> CreateAttachment([FromForm] CreateAttachmentDto createDto, CancellationToken cancellationToken)
    {
        if (createDto.File == null || createDto.File.Length == 0)
            return BadRequest("File is required");

        var createdAttachment = await _attachmentService.CreateAttachmentAsync(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetAttachment), new { id = createdAttachment.Id }, createdAttachment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttachment(Guid id, CancellationToken cancellationToken)
    {
        if (!await _attachmentService.AttachmentExistsAsync(id))
        {
            return NotFound();
        }

        await _attachmentService.DeleteAttachmentAsync(id, cancellationToken);
        return NoContent();
    }
}
