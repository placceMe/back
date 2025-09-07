using Microsoft.AspNetCore.Mvc;
using Marketplace.Contracts.Products;
using Marketplace.Contracts.Common;
using Marketplace.Contracts.Files;
using ProductsService.Models;
using ProductsService.Repositories.Interfaces;
using ProductsService.Extensions;
using ProductsService.Services.Interfaces;
using LocalCreateProductWithFilesDto = ProductsService.DTOs.CreateProductWithFilesDto;
using LocalUpdateProductWithFilesDto = ProductsService.DTOs.UpdateProductWithFilesDto;
using LocalUpdateCharacteristicDto = ProductsService.DTOs.UpdateCharacteristicDto;

namespace ProductsService.Controllers;

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
    public async Task<ActionResult<ApiResponse<ProductsDto>>> GetProducts([FromQuery] PaginationDto paginationDto)
    {
        var products = await _productsService.GetAllProductsAsync(paginationDto.Offset, paginationDto.Limit);
        return Ok(ApiResponse<ProductsDto>.SuccessResult(products.ToContract()));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SearchProductsDto>>>> SearchProducts([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(ApiResponse<IEnumerable<SearchProductsDto>>.ErrorResult("Search query cannot be empty"));
        }

        var products = await _productsService.SearchProductsByTitleAsync(query);
        return Ok(ApiResponse<IEnumerable<SearchProductsDto>>.SuccessResult(products.ToContract()));
    }

    [HttpPost("many")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProductsMany([FromBody] IdsDto dto)
    {
        var products = await _productsService.GetProductsByIdsAsync(dto.Ids);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products.ToDto().ToContract()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(Guid id)
    {
        var product = await _productsService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound(ApiResponse<ProductDto>.ErrorResult("Product not found"));
        }

        return Ok(ApiResponse<ProductDto>.SuccessResult(product.ToDto().ToContract()));
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<ApiResponse<ProductsDto>>> GetProductsByCategoryId(
        Guid categoryId,
        [FromQuery] PaginationDto paginationDto)
    {
        var products = await _productsService.GetProductsByCategoryIdAsync(categoryId, paginationDto.Offset, paginationDto.Limit);
        return Ok(ApiResponse<ProductsDto>.SuccessResult(products.ToContract()));
    }

    [HttpGet("seller/{sellerId}")]
    public async Task<ActionResult<ApiResponse<ProductsDto>>> GetProductsBySellerId(
        Guid sellerId,
        [FromQuery] PaginationDto paginationDto)
    {
        var products = await _productsService.GetProductsBySellerIdAsync(sellerId, paginationDto.Offset, paginationDto.Limit);
        return Ok(ApiResponse<ProductsDto>.SuccessResult(products.ToContract()));
    }

    // Новий endpoint для створення продукту з файлами
    [HttpPost("with-files")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProductWithFiles(
        [FromForm] CreateProductWithFilesDto createProductDto,
        CancellationToken cancellationToken)
    {
        try
        {
            // Мапінг з Contract DTO до Local DTO
            var localDto = new LocalCreateProductWithFilesDto
            {
                Title = createProductDto.Title,
                Producer = createProductDto.Producer,
                IsNew = createProductDto.IsNew,
                Description = createProductDto.Description,
                Price = createProductDto.Price,
                Color = createProductDto.Color,
                Weight = createProductDto.Weight,
                MainImage = createProductDto.MainImage,
                AdditionalImages = createProductDto.AdditionalImages,
                CategoryId = createProductDto.CategoryId,
                SellerId = createProductDto.SellerId,
                Quantity = createProductDto.Quantity,
                Characteristics = createProductDto.Characteristics?.Select(c => new ProductsService.DTOs.CreateCharacteristicDto
                {
                    Value = c.Value,
                    CharacteristicDictId = c.CharacteristicDictId
                }).ToList() ?? new()
            };

            var product = await _productsService.CreateProductWithFilesAsync(localDto, cancellationToken);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, 
                ApiResponse<ProductDto>.SuccessResult(product.ToDto().ToContract(), "Product created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ProductDto>.ErrorResult(ex.Message));
        }
    }

    // Існуючий endpoint залишається для зворотної сумісності
    [HttpPost]
    public ActionResult<ApiResponse<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        // Verify that the category exists
        var category = _categoryRepository.GetCategoryById(createProductDto.CategoryId);
        if (category == null)
        {
            return BadRequest(ApiResponse<ProductDto>.ErrorResult("Category does not exist"));
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
            Quantity = createProductDto.Quantity,
            Producer = createProductDto.Producer,
            IsNew = createProductDto.IsNew
        };

        _productsService.CreateProduct(product);

        // Load the category for the response
        product.Category = category;

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, 
            ApiResponse<ProductDto>.SuccessResult(product.ToDto().ToContract(), "Product created successfully"));
    }

    // Новий endpoint для оновлення продукту з файлами для веб-інтерфейсу
    [HttpPut("{id}/web")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProductForWeb(
        Guid id,
        [FromForm] UpdateProductWithFilesDto updateProductDto,
        CancellationToken cancellationToken)
    {
        try
        {
            // Мапінг з Contract DTO до Local DTO
            var localDto = new LocalUpdateProductWithFilesDto
            {
                Title = updateProductDto.Title,
                Producer = updateProductDto.Producer,
                IsNew = updateProductDto.IsNew,
                Description = updateProductDto.Description,
                Price = updateProductDto.Price,
                Color = updateProductDto.Color,
                Weight = updateProductDto.Weight,
                MainImage = updateProductDto.MainImage,
                AdditionalImages = updateProductDto.AdditionalImages,
                CategoryId = updateProductDto.CategoryId,
                Quantity = updateProductDto.Quantity,
                ImagesToDelete = updateProductDto.ImagesToDelete,
                Characteristics = updateProductDto.Characteristics?.Select(c => new LocalUpdateCharacteristicDto
                {
                    Id = c.Id,
                    Value = c.Value
                }).ToList() ?? new()
            };

            var updatedProduct = await _productsService.UpdateProductWithFilesAsync(id, localDto, cancellationToken);
            
            if (updatedProduct == null)
            {
                return NotFound(ApiResponse<ProductDto>.ErrorResult("Product not found"));
            }

            return Ok(ApiResponse<ProductDto>.SuccessResult(updatedProduct.ToDto().ToContract(), "Product updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ProductDto>.ErrorResult(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductDto updateProductDto)
    {
        // Verify that the category exists
        var category = _categoryRepository.GetCategoryById(updateProductDto.CategoryId);
        if (category == null)
        {
            return BadRequest(ApiResponse.CreateError("Category does not exist"));
        }

        var product = new Product
        {
            Id = id,
            Title = updateProductDto.Title,
            Producer = updateProductDto.Producer,
            IsNew = updateProductDto.IsNew,
            Description = updateProductDto.Description,
            Price = updateProductDto.Price,
            Color = updateProductDto.Color,
            Weight = updateProductDto.Weight,
            MainImageUrl = updateProductDto.MainImageUrl,
            CategoryId = updateProductDto.CategoryId,
            Quantity = updateProductDto.Quantity,
        };

        // Мапінг характеристик
        var localCharacteristics = updateProductDto.Characteristics?.Select(c => new LocalUpdateCharacteristicDto
        {
            Id = c.Id,
            Value = c.Value
        }).ToList();

        var success = await _productsService.UpdateProductAsync(id, product, localCharacteristics);

        if (!success)
        {
            return NotFound(ApiResponse.CreateError("Product not found"));
        }

        return Ok(ApiResponse.CreateSuccess("Product updated successfully"));
    }

    [HttpPut("{id}/state")]
    public async Task<IActionResult> ChangeProductState(Guid id, [FromBody] ChangeProductStateDto dto)
    {
        var success = await _productsService.ChangeProductStateAsync(id, dto.State);
        if (!success)
        {
            return NotFound(ApiResponse.CreateError("Product not found"));
        }
        return Ok(ApiResponse.CreateSuccess("Product state updated successfully"));
    }

    [HttpPut("{id}/quantity")]
    public async Task<IActionResult> ChangeProductQuantity(Guid id, [FromBody] ChangeQuantityDto dto)
    {
        if (!new[] { "add", "minus", "set" }.Contains(dto.Operation.ToLower()))
        {
            return BadRequest(ApiResponse.CreateError("Operation must be 'add', 'minus', or 'set'"));
        }

        if (dto.Quantity < 0)
        {
            return BadRequest(ApiResponse.CreateError("Quantity cannot be negative"));
        }

        var success = await _productsService.ChangeProductQuantityAsync(id, dto.Operation.ToLower(), dto.Quantity);
        if (!success)
        {
            return NotFound(ApiResponse.CreateError("Product not found"));
        }
        return Ok(ApiResponse.CreateSuccess("Product quantity updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var success = await _productsService.DeleteProductAsync(id, cancellationToken);

        if (!success)
        {
            return NotFound(ApiResponse.CreateError("Product not found"));
        }

        return Ok(ApiResponse.CreateSuccess("Product deleted successfully"));
    }

    [HttpGet("state/{state}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProductsByState(string state)
    {
        var products = await _productsService.GetProductsByStateAsync(state);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products.ToDto().ToContract()));
    }

    [HttpGet("filter")]
    public async Task<ActionResult<ApiResponse<ProductsDto>>> GetProductsWithFilter(
        [FromQuery] PaginationDto paginationDto,
        [FromQuery] ProductFilterDto filterDto)
    {
        var products = await _productsService.GetProductsWithFilterAsync(
            paginationDto.Offset, 
            paginationDto.Limit, 
            filterDto.SellerId, 
            filterDto.CategoryId, 
            filterDto.Status);
        return Ok(ApiResponse<ProductsDto>.SuccessResult(products.ToContract()));
    }
}