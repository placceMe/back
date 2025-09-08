using ProductsService.Models;
using Marketplace.Contracts.Products;
using Marketplace.Contracts.Files;
using Marketplace.Contracts.Common;

namespace ProductsService.Extensions;

public static class ProductExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Title = product.Title,
            Producer = product.Producer,
            IsNew = product.IsNew,
            Description = product.Description,
            Price = product.Price,
            Color = product.Color,
            Weight = product.Weight,
            MainImageUrl = product.MainImageUrl,
            CategoryId = product.CategoryId,
            SellerId = product.SellerId,
            State = product.State,
            Quantity = product.Quantity,
            Category = product.Category?.ToDto(),
            AdditionalImageUrls = product.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                Url = a.FilePath // Replace 'FileUrl' with the actual property name that contains the URL in your Attachment model
            }).ToList(),
            Characteristics = product.Characteristics.Select(c => new CharacteristicDto
            {
                Id = c.Id,
                Value = c.Value,
                CharacteristicDictId = c.CharacteristicDictId,
                CharacteristicDict = c.CharacteristicDict != null ? new CharacteristicDictDto
                {
                    Id = c.CharacteristicDict.Id,
                    Name = c.CharacteristicDict.Name ?? string.Empty,
                    Description = string.Empty // No description in local model
                } : null
            }).ToList()
        };
    }

    public static IEnumerable<ProductDto> ToDto(this IEnumerable<Product> products)
    {
        return products.Select(p => p.ToDto());
    }
}
