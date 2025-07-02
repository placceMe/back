using Microsoft.AspNetCore.Mvc;
using ProductsService.Extensions;
using ProductsService.Models;
using ProductsService.Services;

namespace ProductsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        public CategoryController(ICategoryService service)
        {
            _service = service;
        }
        [HttpGet]
        public ActionResult<IEnumerable<Category>> GetAllCategories()
        {
            var categories = _service.GetAllCategories();
            return Ok(categories.ToDto());
        }

        [HttpPost]
        public IActionResult CreateCategory([FromBody] Category category)
        {
            _service.CreateCategory(category);
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCategory(Guid id, [FromBody] Category category)
        {
            var updated = _service.UpdateCategory(id, category);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCategory(Guid id)
        {
            var deleted = _service.DeleteCategory(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}")]
        public ActionResult<Category> GetCategoryById(Guid id)
        {
            var category = _service.GetCategoryById(id);
            if (category == null) return NotFound();
            return Ok(category);
        }
    }
}