using Microsoft.AspNetCore.Mvc;
using ProductsService.DTOs;
using ProductsService.Models;
using ProductsService.Data;
using ProductsService.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ProductsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ProductsDBContext _context;

    public CategoriesController(ProductsDBContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        return Ok(categories.ToDto());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = createCategoryDto.Name,
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category.ToDto());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryDto updateCategoryDto)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        category.Name = updateCategoryDto.Name;
        category.Status = updateCategoryDto.Status;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CategoryExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CategoryExists(Guid id)
    {
        return _context.Categories.Any(e => e.Id == id);
    }
}