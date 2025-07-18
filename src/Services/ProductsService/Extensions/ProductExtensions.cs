using ProductsService.Models;
using ProductsService.DTOs;

namespace ProductsService.Extensions;

public static class ProductExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            Color = product.Color,
            Weight = product.Weight,
            MainImageUrl = product.MainImageUrl,
            CategoryId = product.CategoryId,
            SellerId = product.SellerId,
            State = product.State,
            Quantity = product.Quantity,
            Category = product.Category?.ToDto()
        };
    }

    public static IEnumerable<ProductDto> ToDto(this IEnumerable<Product> products)
    {
        return products.Select(p => p.ToDto());
    }
}