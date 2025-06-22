using Microsoft.AspNetCore.Mvc;
using ProductsService.Models;
using ProductsService.Services;

namespace ProductsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductControllers : ControllerBase
    {
        private readonly IProductsService _service;

        public ProductControllers(IProductsService service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] Product product)
        {
            _service.CreateProduct(product);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(Guid id, [FromBody] Product product)
        {
            var updated = _service.UpdateProduct(id, product);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(Guid id)
        {
            var deleted = _service.DeleteProduct(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetProductById(Guid id)
        {
            var product = _service.GetProductById(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpGet("{productId}/embedding")]
        public ActionResult<ProductEmbedding> GetEmbedding(Guid productId)
        {
            var embedding = _service.GetProductEmbedding(productId);
            if (embedding == null) return NotFound();
            return Ok(embedding);
        }

        [HttpPut("{productId}/embedding")]
        public IActionResult UpdateEmbedding(Guid productId, [FromBody] float[] embedding)
        {
            _service.AddOrUpdateProductEmbedding(new ProductEmbedding { ProductId = productId, Embedding = embedding });
            return Ok();
        }
    }
}