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
                _service.CreateCharacteristicDict(dict);
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
        public IActionResult UpdateCharacteristicDict(Guid id, [FromBody] CharacteristicDict dict)
        {
            var updated = _service.UpdateCharacteristicDict(id, dict);
            if (!updated) return NotFound();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteCharacteristicDict(Guid id)
        {
            var deleted = _service.DeleteCharacteristicDict(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        [HttpGet("{id}")]
        public ActionResult<CharacteristicDict> GetCharacteristicDictById(Guid id)
        {
            var dict = _service.GetCharacteristicDictById(id);
            if (dict == null) return NotFound();
            return Ok(dict);
        }
    }
}