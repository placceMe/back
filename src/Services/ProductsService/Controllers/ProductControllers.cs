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

        [HttpPost("generate-mock-products")]
        public IActionResult GenerateMockProducts()
        {
            var mockProducts = new List<Product>();

            for (int i = 1; i <= 10; i++)
            {
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Title = $"Mock Product {i}",
                    Description = $"This is a mock product description for product {i}",
                    Price = (uint)(100 + i * 50),
                    CategoryId = Guid.NewGuid(),
                    SellerId = Guid.NewGuid(),
                    State = ProductState.Moderation,
                    Quantity = (uint)(10 + i * 5),
                    // Characteristics = new List<Characteristic>(),
                    // Attachments = new List<Attachment>()
                };

                _service.CreateProduct(product);
                mockProducts.Add(product);
            }

            return Ok(new { message = "10 mock products created successfully", products = mockProducts });
        }

        [HttpPost("generate-mock-categories")]
        public IActionResult GenerateMockCategories()
        {
            var mockCategories = new List<object>();
            var categoryNames = new[] { "Electronics", "Clothing", "Books", "Home & Garden", "Sports", "Toys", "Beauty", "Automotive", "Food", "Health" };

            for (int i = 0; i < categoryNames.Length; i++)
            {
                var categoryData = new
                {
                    Id = Guid.NewGuid(),
                    Name = categoryNames[i]
                };

                mockCategories.Add(categoryData);
            }

            return Ok(new { message = $"{categoryNames.Length} mock categories generated", categories = mockCategories });
        }

        [HttpPost("generate-mock-characteristics")]
        public IActionResult GenerateMockCharacteristics()
        {
            var mockCharacteristics = new List<object>();
            var characteristicData = new[]
            {
                ("Color", "Red"), ("Color", "Blue"), ("Color", "Green"),
                ("Size", "Small"), ("Size", "Medium"), ("Size", "Large"),
                ("Material", "Cotton"), ("Material", "Polyester"), ("Material", "Leather"),
                ("Weight", "1kg"), ("Weight", "2kg"), ("Weight", "3kg")
            };

            foreach (var (name, value) in characteristicData)
            {
                var characteristic = new
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Value = value
                };

                mockCharacteristics.Add(characteristic);
            }

            return Ok(new { message = $"{characteristicData.Length} mock characteristics generated", characteristics = mockCharacteristics });
        }
    }
}