using Microsoft.AspNetCore.Mvc;
using ProductsService.DTOs;
using ProductsService.Models;
using ProductsService.Repositories;
using ProductsService.Extensions;

namespace ProductsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductsRepository _productsRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductsController(IProductsRepository productsRepository, ICategoryRepository categoryRepository)
    {
        _productsRepository = productsRepository;
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProductDto>> GetProducts()
    {
        var products = _productsRepository.GetAllProducts();
        return Ok(products.ToDto());
    }

    [HttpGet("{id}")]
    public ActionResult<ProductDto> GetProduct(Guid id)
    {
        var product = _productsRepository.GetProductById(id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product.ToDto());
    }

    [HttpPost]
    public ActionResult<ProductDto> CreateProduct(CreateProductDto createProductDto)
    {
        // Verify that the category exists
        var category = _categoryRepository.GetCategoryById(createProductDto.CategoryId);
        if (category == null)
        {
            return BadRequest("Category does not exist");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = createProductDto.Title,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            Color = createProductDto.Color,
            Weight = createProductDto.Weight,
            MainImageUrl = createProductDto.MainImageUrl,
            CategoryId = createProductDto.CategoryId,
            SellerId = createProductDto.SellerId,
            Quantity = createProductDto.Quantity
        };

        _productsRepository.CreateProduct(product);

        // Load the category for the response
        product.Category = category;

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product.ToDto());
    }

    [HttpPut("{id}")]
    public IActionResult UpdateProduct(Guid id, UpdateProductDto updateProductDto)
    {
        // Verify that the category exists
        var category = _categoryRepository.GetCategoryById(updateProductDto.CategoryId);
        if (category == null)
        {
            return BadRequest("Category does not exist");
        }

        var product = new Product
        {
            Id = id,
            Title = updateProductDto.Title,
            Description = updateProductDto.Description,
            Price = updateProductDto.Price,
            Color = updateProductDto.Color,
            Weight = updateProductDto.Weight,
            MainImageUrl = updateProductDto.MainImageUrl,
            CategoryId = updateProductDto.CategoryId,
            Quantity = updateProductDto.Quantity
        };

        var success = _productsRepository.UpdateProduct(id, product);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(Guid id)
    {
        var success = _productsRepository.DeleteProduct(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}