using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacteristicDictController : ControllerBase
    {
        private readonly ICharacteristicDictService _service;
        public CharacteristicDictController(ICharacteristicDictService service)
        {
            _service = service;
        }
        [HttpPost]
        public IActionResult CreateCharacteristicDict([FromBody] CharacteristicDict dict)
        {
            _service.CreateCharacteristicDict(dict);
            return Ok();
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