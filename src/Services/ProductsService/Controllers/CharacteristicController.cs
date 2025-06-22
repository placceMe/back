using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacteristicController : ControllerBase
    {
        private readonly ICharacteristicService _service;
        public CharacteristicController(ICharacteristicService service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult CreateCharacteristic([FromBody] Characteristic characteristic)
        {
            _service.CreateCharacteristic(characteristic);
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCharacteristic(Guid id, [FromBody] Characteristic characteristic)
        {
            var updated = _service.UpdateCharacteristic(id, characteristic);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCharacteristic(Guid id)
        {
            var deleted = _service.DeleteCharacteristic(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}")]
        public ActionResult<Characteristic> GetCharacteristicById(Guid id)
        {
            var characteristic = _service.GetCharacteristicById(id);
            if (characteristic == null) return NotFound();
            return Ok(characteristic);
        }
    }
}