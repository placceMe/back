using ProductsService.Models;
using ContractProductDto = Marketplace.Contracts.Products.ProductDto;
using ContractProductsDto = Marketplace.Contracts.Products.ProductsDto;
using ContractSearchProductsDto = Marketplace.Contracts.Products.SearchProductsDto;
using ContractCategoryDto = Marketplace.Contracts.Products.CategoryDto;
using ContractCharacteristicDto = Marketplace.Contracts.Products.CharacteristicDto;
using ContractAttachmentDto = Marketplace.Contracts.Files.AttachmentDto;
using ContractPaginationInfo = Marketplace.Contracts.Common.PaginationInfo;
using LocalProductDto = Marketplace.Contracts.Products.ProductDto;
using LocalProductsDto = Marketplace.Contracts.Products.ProductsDto;
using LocalSearchProductsDto = Marketplace.Contracts.Products.SearchProductsDto;
using LocalCategoryDto = Marketplace.Contracts.Products.CategoryDto;
using LocalCharacteristicDto = Marketplace.Contracts.Products.CharacteristicDto;
using LocalAttachmentDto = Marketplace.Contracts.Files.AttachmentDto;
using LocalPaginationInfo = Marketplace.Contracts.Common.PaginationInfo;

namespace ProductsService.Extensions;

public static class ContractMappingExtensions
{
    // Product mappings
    public static ContractProductDto ToContract(this LocalProductDto localDto)
    {
        return new ContractProductDto
        {
            Id = localDto.Id,
            Title = localDto.Title,
            Producer = localDto.Producer,
            IsNew = localDto.IsNew,
            Description = localDto.Description,
            Price = localDto.Price,
            Color = localDto.Color,
            Weight = localDto.Weight,
            MainImageUrl = localDto.MainImageUrl,
            CategoryId = localDto.CategoryId,
            SellerId = localDto.SellerId,
            State = localDto.State,
            Quantity = localDto.Quantity,
            Category = localDto.Category?.ToContract(),
            AdditionalImageUrls = localDto.AdditionalImageUrls?.Select(a => a.ToContract()).ToList() ?? new(),
            Characteristics = localDto.Characteristics?.Select(c => c.ToContract()).ToList() ?? new()
        };
    }

    public static Product ToModel(this ContractProductDto contractDto)
    {
        return new Product
        {
            Id = contractDto.Id,
            Title = contractDto.Title,
            Producer = contractDto.Producer,
            IsNew = contractDto.IsNew,
            Description = contractDto.Description,
            Price = contractDto.Price,
            Color = contractDto.Color,
            Weight = contractDto.Weight,
            MainImageUrl = contractDto.MainImageUrl,
            CategoryId = contractDto.CategoryId,
            SellerId = contractDto.SellerId,
            State = contractDto.State,
            Quantity = contractDto.Quantity
        };
    }

    public static IEnumerable<ContractProductDto> ToContract(this IEnumerable<LocalProductDto> localDtos)
    {
        return localDtos.Select(dto => dto.ToContract());
    }

    // ProductsDto mappings
    public static ContractProductsDto ToContract(this LocalProductsDto localDto)
    {
        return new ContractProductsDto
        {
            Products = localDto.Products?.Select(p => p.ToContract()).ToList() ?? new(),
            Pagination = localDto.Pagination?.ToContract() ?? new()
        };
    }

    // SearchProductsDto mappings
    public static ContractSearchProductsDto ToContract(this LocalSearchProductsDto localDto)
    {
        return new ContractSearchProductsDto
        {
            Id = localDto.Id,
            Title = localDto.Title,
            Description = localDto.Description,
            Price = localDto.Price,
            Color = localDto.Color,
            MainImageUrl = localDto.MainImageUrl
        };
    }

    public static IEnumerable<ContractSearchProductsDto> ToContract(this IEnumerable<LocalSearchProductsDto> localDtos)
    {
        return localDtos.Select(dto => dto.ToContract());
    }

    // Category mappings
    public static ContractCategoryDto ToContract(this LocalCategoryDto localDto)
    {
        return new ContractCategoryDto
        {
            Id = localDto.Id,
            Name = localDto.Name,
            Description = string.Empty, // Local DTO doesn't have Description
            ParentCategoryId = null, // Local DTO doesn't have ParentCategoryId
            ImageUrl = string.Empty, // Local DTO doesn't have ImageUrl
            SubCategories = new List<ContractCategoryDto>() // Local DTO doesn't have SubCategories
        };
    }

    // Characteristic mappings
    public static ContractCharacteristicDto ToContract(this LocalCharacteristicDto localDto)
    {
        return new ContractCharacteristicDto
        {
            Id = localDto.Id,
            Value = localDto.Value,
            CharacteristicDictId = localDto.CharacteristicDictId,
            CharacteristicDict = new Marketplace.Contracts.Products.CharacteristicDictDto
            {
                Id = localDto.CharacteristicDictId,
                Name = localDto.Name, // Use Name from local DTO as Name
                Description = string.Empty
            }
        };
    }

    // Attachment mappings
    public static ContractAttachmentDto ToContract(this LocalAttachmentDto localDto)
    {
        return new ContractAttachmentDto
        {
            Id = localDto.Id,
            Url = localDto.Url,
            FileName = string.Empty, // Local DTO doesn't have FileName
            Size = 0, // Local DTO doesn't have Size
            ContentType = string.Empty, // Local DTO doesn't have ContentType
            ProductId = Guid.Empty, // Local DTO doesn't have ProductId
            CreatedAt = DateTime.UtcNow
        };
    }

    // PaginationInfo mappings
    public static ContractPaginationInfo ToContract(this LocalPaginationInfo localInfo)
    {
        return new ContractPaginationInfo
        {
            TotalItems = localInfo.TotalItems,
            TotalPages = localInfo.TotalPages,
            CurrentPage = localInfo.CurrentPage,
            PageSize = localInfo.PageSize,
            HasNextPage = localInfo.CurrentPage < localInfo.TotalPages, // Calculate from available data
            HasPreviousPage = localInfo.CurrentPage > 1 // Calculate from available data
        };
    }
}
