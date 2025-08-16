using Microsoft.AspNetCore.Mvc;
using ProductsService.DTOs;
using ProductsService.Services.Interfaces;

namespace ProductsService.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetAllFeedbacks([FromQuery] PaginationDto paginationDto)
    {
        try
        {
            var feedbacks = await _feedbackService.GetAllFeedbacksAsync(paginationDto.Offset, paginationDto.Limit);
            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all feedbacks");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FeedbackDto>> GetFeedback(Guid id)
    {
        try
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }
            return Ok(feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feedback {FeedbackId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedbacksByProduct(
        Guid productId,
        [FromQuery] PaginationDto paginationDto)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbacksByProductIdAsync(productId, paginationDto.Offset, paginationDto.Limit);
            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feedbacks for product {ProductId}", productId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedbacksByUser(
        Guid userId,
        [FromQuery] PaginationDto paginationDto)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbacksByUserIdAsync(userId, paginationDto.Offset, paginationDto.Limit);
            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feedbacks for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("product/{productId}/summary")]
    public async Task<ActionResult<FeedbackSummaryDto>> GetFeedbackSummary(Guid productId)
    {
        try
        {
            var summary = await _feedbackService.GetFeedbackSummaryByProductIdAsync(productId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feedback summary for product {ProductId}", productId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<FeedbackDto>> CreateFeedback([FromBody] CreateFeedbackDto createFeedbackDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var feedback = await _feedbackService.CreateFeedbackAsync(createFeedbackDto);
            return CreatedAtAction(nameof(GetFeedback), new { id = feedback.Id }, feedback);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating feedback");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feedback");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FeedbackDto>> UpdateFeedback(Guid id, [FromBody] UpdateFeedbackDto updateFeedbackDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var feedback = await _feedbackService.UpdateFeedbackAsync(id, updateFeedbackDto);
            return Ok(feedback);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when updating feedback {FeedbackId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feedback {FeedbackId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFeedback(Guid id)
    {
        try
        {
            var result = await _feedbackService.DeleteFeedbackAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feedback {FeedbackId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}