using ProductsService.Models;
using ProductsService.DTOs;

namespace ProductsService.Extensions;

public static class CategoryExtensions
{
    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Status = category.Status
        };
    }

    public static IEnumerable<CategoryDto> ToDto(this IEnumerable<Category> categories)
    {
        return categories.Select(c => c.ToDto());
    }
}