using Microsoft.AspNetCore.Mvc;
using ProductsService.DTOs;
using ProductsService.Services.Interfaces;

namespace ProductsService.Controllers;

[ApiController]
[Route("api/products/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly ILogger<FeedbackController> _logger;

    public FeedbackController(IFeedbackService feedbackService, ILogger<FeedbackController> logger)
    {
        _feedbackService = feedbackService;

        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetAllFeedbacks(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50)
    {
        var feedbacks = await _feedbackService.GetAllFeedbacksAsync(offset, limit);
        return Ok(feedbacks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FeedbackDto>> GetFeedbackById(Guid id)
    {
        var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
        if (feedback == null)
            return NotFound();

        return Ok(feedback);
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedbacksByProductId(
        Guid productId,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50)
    {
        var feedbacks = await _feedbackService.GetFeedbacksByProductIdAsync(productId, offset, limit);
        return Ok(feedbacks);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedbacksByUserId(
        Guid userId,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50)
    {
        var feedbacks = await _feedbackService.GetFeedbacksByUserIdAsync(userId, offset, limit);
        return Ok(feedbacks);
    }

    [HttpGet("saler/{sellerId}")]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedbacksBySellerId(
        Guid sellerId,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50)
    {
        var feedbacks = await _feedbackService.GetFeedbacksBySellerIdAsync(sellerId, offset, limit);
        return Ok(feedbacks);
    }

    [HttpGet("product/{productId}/summary")]
    public async Task<ActionResult<FeedbackSummaryDto>> GetFeedbackSummaryByProductId(Guid productId)
    {
        var summary = await _feedbackService.GetFeedbackSummaryByProductIdAsync(productId);
        return Ok(summary);
    }

    [HttpPost]
    public async Task<ActionResult<FeedbackDto>> CreateFeedback([FromBody] CreateFeedbackDto createFeedbackDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var feedback = await _feedbackService.CreateFeedbackAsync(createFeedbackDto);
        return CreatedAtAction(nameof(GetFeedbackById), new { id = feedback.Id }, feedback);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FeedbackDto>> UpdateFeedback(Guid id, [FromBody] UpdateFeedbackDto updateFeedbackDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var feedback = await _feedbackService.UpdateFeedbackAsync(id, updateFeedbackDto);
            return Ok(feedback);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFeedback(Guid id)
    {
        var deleted = await _feedbackService.DeleteFeedbackAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<FeedbackDto>> UpdateFeedbackStatus(Guid id, [FromBody] UpdateFeedbackStatusDto updateStatusDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var feedback = await _feedbackService.UpdateFeedbackStatusAsync(id, updateStatusDto);
            return Ok(feedback);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
}