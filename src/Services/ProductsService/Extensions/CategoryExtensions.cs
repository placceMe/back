using ProductsService.Models;
using Marketplace.Contracts.Products;
using Marketplace.Contracts.Files;
using Marketplace.Contracts.Common;

namespace ProductsService.Extensions;

public static class CategoryExtensions
{
    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = string.Empty, // Category model doesn't have Description
            ParentCategoryId = null, // Category model doesn't have ParentCategoryId
            ImageUrl = string.Empty // Category model doesn't have ImageUrl
        };
    }

    public static IEnumerable<CategoryDto> ToDto(this IEnumerable<Category> categories)
    {
        return categories.Select(c => c.ToDto());
    }
}



