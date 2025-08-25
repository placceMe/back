using Microsoft.AspNetCore.Mvc;
using ProductsService.DTOs;
using ProductsService.Models;
using ProductsService.Repositories.Interfaces;
using ProductsService.Extensions;
using ProductsService.Services.Interfaces;

namespace ProductsService.Controllers;

public class IdsDto
{
    public IEnumerable<Guid> Ids { get; set; }
}

public class ChangeProductStateDto
{
    public string State { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductsService _productsService;

    public ProductsController(
        ICategoryRepository categoryRepository,
        IProductsService productsService)
    {
        _categoryRepository = categoryRepository;
        _productsService = productsService;
    }

    [HttpGet]
    public async Task<ActionResult<ProductsDto>> GetProducts([FromQuery] PaginationDto paginationDto)
    {
        var products = await _productsService.GetAllProductsAsync(paginationDto.Offset, paginationDto.Limit);
        return Ok(products);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<SearchProductsDto>>> SearchProducts([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Search query cannot be empty");
        }

        var products = await _productsService.SearchProductsByTitleAsync(query);
        return Ok(products);
    }

    [HttpPost("many")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsMany([FromBody] IdsDto dto)
    {
        var products = await _productsService.GetProductsByIdsAsync(dto.Ids);
        return Ok(products.ToDto());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _productsService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product.ToDto());
    }
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<ProductsDto>> GetProductsByCategoryId(
        Guid categoryId,
        [FromQuery] PaginationDto paginationDto)
    {
        var products = await _productsService.GetProductsByCategoryIdAsync(categoryId, paginationDto.Offset, paginationDto.Limit);
        return Ok(products);
    }

    [HttpGet("seller/{sellerId}")]
    public async Task<ActionResult<ProductsDto>> GetProductsBySellerId(
        Guid sellerId,
        [FromQuery] PaginationDto paginationDto)
    {
        var products = await _productsService.GetProductsBySellerIdAsync(sellerId, paginationDto.Offset, paginationDto.Limit);
        return Ok(products);
    }

    // Новий endpoint для створення продукту з файлами
    [HttpPost("with-files")]
    public async Task<ActionResult<ProductDto>> CreateProductWithFiles(
        [FromForm] CreateProductWithFilesDto createProductDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productsService.CreateProductWithFilesAsync(createProductDto, cancellationToken);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product.ToDto());
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Існуючий endpoint залишається для зворотної сумісності
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

        _productsService.CreateProduct(product);

        // Load the category for the response
        product.Category = category;

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product.ToDto());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductDto updateProductDto)
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

        var success = await _productsService.UpdateProductAsync(id, product, updateProductDto.Characteristics);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPut("{id}/state")]
    public async Task<IActionResult> ChangeProductState(Guid id, [FromBody] ChangeProductStateDto dto)
    {
        var success = await _productsService.ChangeProductStateAsync(id, dto.State);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var success = await _productsService.DeleteProductAsync(id, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("state/{state}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByState(string state)
    {
        var products = await _productsService.GetProductsByStateAsync(state);
        return Ok(products.ToDto());
    }
}