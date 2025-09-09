using Microsoft.AspNetCore.Mvc;
using ContentService.DTOs;
using ContentService.Services;

namespace ContentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaqController : ControllerBase
{
    private readonly IFaqService _faqService;
    private readonly ILogger<FaqController> _logger;

    public FaqController(IFaqService faqService, ILogger<FaqController> logger)
    {
        _faqService = faqService;
        _logger = logger;
    }

    /// <summary>
    /// Get all FAQs (Admin only - authorization will be added later)
    /// </summary>
    [HttpGet]
    // [Authorize(Roles = "Admin")] - додамо пізніше
    public async Task<ActionResult<IEnumerable<FaqDto>>> GetAllFaqs()
    {
        try
        {
            var faqs = await _faqService.GetAllFaqsAsync();
            return Ok(faqs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all FAQs");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get visible FAQs (Public endpoint)
    /// </summary>
    [HttpGet("visible")]
    public async Task<ActionResult<IEnumerable<FaqDto>>> GetVisibleFaqs()
    {
        try
        {
            var faqs = await _faqService.GetVisibleFaqsAsync();
            return Ok(faqs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visible FAQs");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get FAQ by ID (Admin only - authorization will be added later)
    /// </summary>
    [HttpGet("{id}")]
    // [Authorize(Roles = "Admin")] - додамо пізніше
    public async Task<ActionResult<FaqDto>> GetFaqById(int id)
    {
        try
        {
            var faq = await _faqService.GetFaqByIdAsync(id);
            if (faq == null)
                return NotFound($"FAQ with ID {id} not found");

            return Ok(faq);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FAQ by ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create new FAQ (Admin only - authorization will be added later)
    /// </summary>
    [HttpPost]
    // [Authorize(Roles = "Admin")] - додамо пізніше
    public async Task<ActionResult<FaqDto>> CreateFaq([FromBody] CreateFaqDto createFaqDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = "admin"; // Тимчасово, буде замінено на справжнього юзера
            var faq = await _faqService.CreateFaqAsync(createFaqDto, userId);

            return CreatedAtAction(nameof(GetFaqById), new { id = faq.Id }, faq);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating FAQ");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update existing FAQ (Admin only - authorization will be added later)
    /// </summary>
    [HttpPut("{id}")]
    // [Authorize(Roles = "Admin")] - додамо пізніше
    public async Task<ActionResult<FaqDto>> UpdateFaq(int id, [FromBody] UpdateFaqDto updateFaqDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = "admin"; // Тимчасово, буде замінено на справжнього юзера
            var faq = await _faqService.UpdateFaqAsync(id, updateFaqDto, userId);

            if (faq == null)
                return NotFound($"FAQ with ID {id} not found");

            return Ok(faq);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating FAQ with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete FAQ (Admin only - authorization will be added later)
    /// </summary>
    [HttpDelete("{id}")]
    // [Authorize(Roles = "Admin")] - додамо пізніше
    public async Task<ActionResult> DeleteFaq(int id)
    {
        try
        {
            var success = await _faqService.DeleteFaqAsync(id);
            if (!success)
                return NotFound($"FAQ with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting FAQ with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
