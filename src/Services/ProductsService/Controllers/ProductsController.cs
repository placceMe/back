using Microsoft.AspNetCore.Mvc;
using ProductsService.DTOs;
using ProductsService.Models;
using ProductsService.Data;
using Microsoft.EntityFrameworkCore;

namespace ProductsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductsDBContext _context;

    public ProductsController(ProductsDBContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Color = p.Color,
                Weight = p.Weight,
                CategoryId = p.CategoryId,
                SellerId = p.SellerId,
                State = p.State,
                Quantity = p.Quantity,
                Category = p.Category != null ? new CategoryDto
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Status = p.Category.Status
                } : null
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        var productDto = new ProductDto
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            Color = product.Color,
            Weight = product.Weight,
            CategoryId = product.CategoryId,
            SellerId = product.SellerId,
            State = product.State,
            Quantity = product.Quantity,
            Category = product.Category != null ? new CategoryDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                Status = product.Category.Status
            } : null
        };

        return Ok(productDto);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        // Verify that the category exists
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == createProductDto.CategoryId);
        if (!categoryExists)
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
            CategoryId = createProductDto.CategoryId,
            SellerId = createProductDto.SellerId,
            Quantity = createProductDto.Quantity
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Load the category for the response
        await _context.Entry(product)
            .Reference(p => p.Category)
            .LoadAsync();

        var productDto = new ProductDto
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            Color = product.Color,
            Weight = product.Weight,
            CategoryId = product.CategoryId,
            SellerId = product.SellerId,
            State = product.State,
            Quantity = product.Quantity,
            Category = product.Category != null ? new CategoryDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                Status = product.Category.Status
            } : null
        };

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductDto updateProductDto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        // Verify that the category exists
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateProductDto.CategoryId);
        if (!categoryExists)
        {
            return BadRequest("Category does not exist");
        }

        product.Title = updateProductDto.Title;
        product.Description = updateProductDto.Description;
        product.Price = updateProductDto.Price;
        product.Color = updateProductDto.Color;
        product.Weight = updateProductDto.Weight;
        product.CategoryId = updateProductDto.CategoryId;
        product.Quantity = updateProductDto.Quantity;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProductExists(Guid id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}