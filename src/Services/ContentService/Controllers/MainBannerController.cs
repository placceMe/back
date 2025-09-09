using ContentService.DTOs;
using ContentService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MainBannerController : ControllerBase
{
    private readonly IMainBannerService _mainBannerService;
    private readonly ILogger<MainBannerController> _logger;

    public MainBannerController(IMainBannerService mainBannerService, ILogger<MainBannerController> logger)
    {
        _mainBannerService = mainBannerService;
        _logger = logger;
    }

    /// <summary>
    /// Отримати всі банери (для адміністраторів)
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
            _logger.LogError(ex, "Помилка при отриманні всіх банерів");
            return StatusCode(500, "Внутрішня помилка сервера");
        }
    }

    /// <summary>
    /// Отримати тільки видимі банери (публічний ендпоінт)
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
            _logger.LogError(ex, "Помилка при отриманні видимих банерів");
            return StatusCode(500, "Внутрішня помилка сервера");
        }
    }

    /// <summary>
    /// Отримати банер за ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MainBannerDto>> GetById(int id)
    {
        try
        {
            var banner = await _mainBannerService.GetByIdAsync(id);
            if (banner == null)
            {
                return NotFound($"Банер з ID {id} не знайдено");
            }

            return Ok(banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні банера з ID {BannerId}", id);
            return StatusCode(500, "Внутрішня помилка сервера");
        }
    }

    /// <summary>
    /// Створити новий банер
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MainBannerDto>> Create([FromForm] CreateMainBannerDto createDto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Перевірити, чи файл зображення надано
            if (createDto.Image == null || createDto.Image.Length == 0)
            {
                return BadRequest("Зображення є обов'язковим");
            }

            // Перевірити тип файлу
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(createDto.Image.ContentType.ToLower()))
            {
                return BadRequest("Дозволені тільки файли зображень (JPEG, PNG, GIF, WebP)");
            }

            // Перевірити розмір файлу (максимум 5MB)
            if (createDto.Image.Length > 5 * 1024 * 1024)
            {
                return BadRequest("Розмір файлу не повинен перевищувати 5MB");
            }

            var banner = await _mainBannerService.CreateAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = banner.Id }, banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні банера");
            return StatusCode(500, "Внутрішня помилка сервера");
        }
    }

    /// <summary>
    /// Оновити банер
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MainBannerDto>> Update(int id, [FromForm] UpdateMainBannerDto updateDto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Якщо надано нове зображення, перевірити його
            if (updateDto.Image != null)
            {
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(updateDto.Image.ContentType.ToLower()))
                {
                    return BadRequest("Дозволені тільки файли зображень (JPEG, PNG, GIF, WebP)");
                }

                if (updateDto.Image.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("Розмір файлу не повинен перевищувати 5MB");
                }
            }

            var banner = await _mainBannerService.UpdateAsync(id, updateDto, cancellationToken);
            if (banner == null)
            {
                return NotFound($"Банер з ID {id} не знайдено");
            }

            return Ok(banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні банера з ID {BannerId}", id);
            return StatusCode(500, "Внутрішня помилка сервера");
        }
    }

    /// <summary>
    /// Видалити банер
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mainBannerService.DeleteAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound($"Банер з ID {id} не знайдено");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при видаленні банера з ID {BannerId}", id);
            return StatusCode(500, "Внутрішня помилка сервера");
        }
    }

    /// <summary>
    /// Змінити видимість банера
    /// </summary>
    [HttpPatch("{id}/visibility")]
    public async Task<ActionResult<MainBannerDto>> ToggleVisibility(int id, [FromBody] bool isVisible, CancellationToken cancellationToken)
    {
        try
        {
            var updateDto = new UpdateMainBannerDto
            {
                IsVisible = isVisible
            };

            var banner = await _mainBannerService.UpdateAsync(id, updateDto, cancellationToken);
            if (banner == null)
            {
                return NotFound($"Банер з ID {id} не знайдено");
            }

            return Ok(banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при зміні видимості банера з ID {BannerId}", id);
            return StatusCode(500, "Внутрішня помилка сервера");
        }
    }

    /// <summary>
    /// Змінити порядок банера
    /// </summary>
    [HttpPatch("{id}/order")]
    public async Task<ActionResult<MainBannerDto>> UpdateOrder(int id, [FromBody] int order, CancellationToken cancellationToken)
    {
        try
        {
            var updateDto = new UpdateMainBannerDto
            {
                Order = order
            };

            var banner = await _mainBannerService.UpdateAsync(id, updateDto, cancellationToken);
            if (banner == null)
            {
                return NotFound($"Банер з ID {id} не знайдено");
            }

            return Ok(banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при зміні порядку банера з ID {BannerId}", id);
            return StatusCode(500, "Внутрішня помилка сервера");
        }
    }
}
