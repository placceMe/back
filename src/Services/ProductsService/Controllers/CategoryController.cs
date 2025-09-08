using Microsoft.AspNetCore.Mvc;
using ProductsService.Extensions;
using ProductsService.Models;
using Marketplace.Contracts.Products;
using Marketplace.Contracts.Files;
using Marketplace.Contracts.Common;
using ProductsService.Services.Interfaces;

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
        [HttpPut("state/{id}")]
        public async Task<IActionResult> UpdateCategoryState(Guid id, [FromBody] CategoryStateDto stateDto)
        {
            var updated = await _service.UpdateCategoryState(id, stateDto);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("transfer/{id}")]
        public async Task<IActionResult> DeleteCategoryWithTransfer(Guid id, [FromBody] DeleteCategoryDto deleteDto)
        {
            var deleted = await _service.DeleteCategoryWithTransfer(id, deleteDto);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("full-info")]
        public async Task<ActionResult<IEnumerable<CategoryFullInfo>>> GetAllCategoriesFullInfo()
        {
            var categories = await _service.GetAllCategoriesFullInfo();
            return Ok(categories);
        }
    }


}
