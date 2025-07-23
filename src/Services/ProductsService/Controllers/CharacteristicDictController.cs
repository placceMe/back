using Microsoft.AspNetCore.Mvc;
using ProductsService.Models;
using ProductsService.Services;

namespace ProductsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacteristicDictController : ControllerBase
    {
        private readonly ICharacteristicDictService _service;
        private readonly ILogger<CharacteristicDictController> _logger;

        public CharacteristicDictController(ICharacteristicDictService service, ILogger<CharacteristicDictController> logger)
        {
            _service = service;
            _logger = logger;
        }
        [HttpPost]
        public IActionResult CreateCharacteristicDict([FromBody] CharacteristicDict dict)
        {
            _logger.LogInformation("POST /api/characteristicdict called with Name: {Name}", dict?.Name);

            if (dict == null)
            {
                _logger.LogWarning("Received null CharacteristicDict");
                return BadRequest("CharacteristicDict cannot be null");
            }

            try
            {
                _service.CreateCharacteristicDictAsync(dict);
                _logger.LogInformation("CharacteristicDict created successfully");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating CharacteristicDict");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCharacteristicDict(Guid id, [FromBody] CharacteristicDict dict)
        {
            var updated = await _service.UpdateCharacteristicDictAsync(id, dict);
            if (!updated) return NotFound();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCharacteristicDict(Guid id)
        {
            var deleted = await _service.DeleteCharacteristicDictAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<CharacteristicDict>> GetCharacteristicDictById(Guid id)
        {
            var dict = await _service.GetCharacteristicDictByIdAsync(id);
            if (dict == null) return NotFound();
            return Ok(dict);
        }
    }
}