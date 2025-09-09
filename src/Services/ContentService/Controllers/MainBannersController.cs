using ContentService.DTOs;
using ContentService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MainBannersController : ControllerBase
{
    private readonly IMainBannerService _mainBannerService;
    private readonly ILogger<MainBannersController> _logger;

    public MainBannersController(IMainBannerService mainBannerService, ILogger<MainBannersController> logger)
    {
        _mainBannerService = mainBannerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all main banners (admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MainBannerDto>>> GetAll()
    {
        try
        {
            var banners = await _mainBannerService.GetAllAsync();
            return Ok(banners);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all main banners");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get visible main banners (public endpoint)
    /// </summary>
    [HttpGet("visible")]
    public async Task<ActionResult<IEnumerable<MainBannerDto>>> GetVisible()
    {
        try
        {
            var banners = await _mainBannerService.GetVisibleAsync();
            return Ok(banners);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving visible main banners");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a main banner by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MainBannerDto>> GetById(int id)
    {
        try
        {
            var banner = await _mainBannerService.GetByIdAsync(id);
            if (banner == null)
                return NotFound();

            return Ok(banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving main banner {BannerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new main banner
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MainBannerDto>> Create([FromForm] CreateMainBannerDto createDto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var banner = await _mainBannerService.CreateAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = banner.Id }, banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating main banner");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a main banner
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MainBannerDto>> Update(int id, [FromForm] UpdateMainBannerDto updateDto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var banner = await _mainBannerService.UpdateAsync(id, updateDto, cancellationToken);
            if (banner == null)
                return NotFound();

            return Ok(banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating main banner {BannerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a main banner
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mainBannerService.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting main banner {BannerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
