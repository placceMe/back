using ProductsService.Models;
using ProductsService.DTOs;
using ProductsService.Repositories.Interfaces;
using ProductsService.Services.Interfaces;
using ProductsService.Extensions;

namespace ProductsService.Services;

public class ProductsService : IProductsService
{
    private readonly IProductsRepository _repository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFilesServiceClient _filesServiceClient;
    private readonly UsersServiceClient _usersServiceClient;
    private readonly IAttachmentService _attachmentService;
    private readonly ICharacteristicService _characteristicService;
    private readonly ILogger<ProductsService> _logger;

    public ProductsService(
        IProductsRepository repository,
        ICategoryRepository categoryRepository,
        IFilesServiceClient filesServiceClient,
        IAttachmentService attachmentService,
        ICharacteristicService characteristicService,
        UsersServiceClient usersServiceClient,
        ILogger<ProductsService> logger)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _filesServiceClient = filesServiceClient;
        _usersServiceClient = usersServiceClient;
        _attachmentService = attachmentService;
        _characteristicService = characteristicService;
        _logger = logger;
    }

    public void CreateProduct(Product product)
    {
        _repository.CreateProduct(product);
        // Створюємо embedding з дефолтним вектором (наприклад, 0-і)
        var embedding = new ProductEmbedding
        {
            ProductId = product.Id,
            Embedding = new float[128] // або інша довжина за замовчуванням
        };
        _repository.AddOrUpdateProductEmbedding(embedding);
    }

    // Новий метод для створення продукту з файлами
    public async Task<Product> CreateProductWithFilesAsync(CreateProductWithFilesDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Крок 1: Перевірити, чи існує категорія
            var category = _categoryRepository.GetCategoryById(createDto.CategoryId);
            if (category == null)
            {
                throw new ArgumentException($"Category with id {createDto.CategoryId} does not exist");
            }

            // Крок 2: Створити продукт без MainImageUrl
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Title = createDto.Title,
                Description = createDto.Description,
                Price = createDto.Price,
                Color = createDto.Color,
                Weight = createDto.Weight,
                MainImageUrl = string.Empty, // Поки що пусто
                CategoryId = createDto.CategoryId,
                SellerId = createDto.SellerId,
                Quantity = createDto.Quantity,
                Category = category,
               Producer = createDto.Producer,
                IsNew = createDto.IsNew,

            };

            // Створюємо продукт (включаючи embedding)
            CreateProduct(product);
            // Крок 3: Створити характеристики продукту
            if (createDto.Characteristics != null && createDto.Characteristics.Any())
            {
                var characteristics = createDto.Characteristics.Select(charDto => new Characteristic
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Value = charDto.Value,
                    CharacteristicDictId = charDto.CharacteristicDictId
                }).ToList();

                foreach (var characteristic in characteristics)
                {
                    await _characteristicService.CreateCharacteristicAsync(characteristic);
                }

            }

            // Крок 4: Завантажити головне зображення, якщо є
            if (createDto.MainImage != null)
            {
                var mainImageFileName = await _filesServiceClient.UploadImageAsync(createDto.MainImage, cancellationToken);

                // Оновити продукт з MainImageUrl
                product.MainImageUrl = mainImageFileName;
                _repository.UpdateProduct(product.Id, product);
            }

            // Крок 5: Створити додаткові зображення через AttachmentService
            if (createDto.AdditionalImages != null && createDto.AdditionalImages.Any())
            {
                foreach (var image in createDto.AdditionalImages)
                {
                    var attachmentDto = new CreateAttachmentDto
                    {
                        File = image,
                        ProductId = product.Id
                    };
                    await _attachmentService.CreateAttachmentAsync(attachmentDto, cancellationToken);
                }
            }
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product with files for seller {SellerId}", createDto.SellerId);
            throw;
        }
    }



    public bool DeleteProduct(Guid id)
    {
        return _repository.DeleteProduct(id);
    }

    // Новий асинхронний метод видалення з файлами
    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = _repository.GetProductById(id);
        if (product == null)
        {
            return false;
        }

        try
        {
            // Видалити головне зображення з FilesService
            if (!string.IsNullOrEmpty(product.MainImageUrl))
            {
                await _filesServiceClient.DeleteImageAsync(product.MainImageUrl, cancellationToken);
            }

            // Видалити всі прикріплення (AttachmentService автоматично видалить файли)
            var attachments = await _attachmentService.GetAttachmentsByProductIdAsync(id);
            var deleteTasks = attachments.Select(a => _attachmentService.DeleteAttachmentAsync(a.Id, cancellationToken));
            await Task.WhenAll(deleteTasks);

            // Видалити продукт
            return _repository.DeleteProduct(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete product {ProductId}", id);
            throw;
        }
    }

    public Product? GetProductById(Guid id)
    {
        return _repository.GetProductById(id);
    }

    // Нові асинхронні методи
    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await Task.FromResult(_repository.GetProductById(id));
    }

    public async Task<ProductsDto> GetAllProductsAsync(int offset, int limit)
    {
        var products = await _repository.GetAllProductsAsync(offset, limit);

        var info = await _repository.GetPaginationInfoAsync(offset, limit, null);
        var productsDto = new ProductsDto
        {
            Products = products.ToDto()?.ToList() ?? new List<ProductDto>(),
            Pagination = info
        };
        return productsDto;
    }

    public ProductEmbedding? GetProductEmbedding(Guid productId)
    {
        return _repository.GetProductEmbedding(productId);
    }

    public void AddOrUpdateProductEmbedding(ProductEmbedding embedding)
    {
        _repository.AddOrUpdateProductEmbedding(embedding);
    }

    public Task<IEnumerable<Product>> GetProductsByIdsAsync(IEnumerable<Guid> ids)
    {
        var products = _repository.GetAllProducts().Where(p => ids.Contains(p.Id));
        return Task.FromResult(products);
    }

    public async Task<ProductsDto> GetProductsByCategoryIdAsync(Guid categoryId, int offset, int limit)
    {

        var products = await _repository.GetProductsByCategoryIdAsync(categoryId, offset, limit);

        var info = await _repository.GetPaginationInfoAsync(offset, limit, categoryId);
        var productsDto = new ProductsDto
        {
            Products = products.ToDto()?.ToList() ?? new List<ProductDto>(),
            Pagination = info
        };


        return productsDto;
    }

    public async Task<IEnumerable<SearchProductsDto>> SearchProductsByTitleAsync(string query)
    {
        var products = await _repository.SearchProductsByTitleAsync(query);
        return products.Select(p => new SearchProductsDto
        {
            Title = p.Title,
            Description = p.Description,
            Price = p.Price,
            Color = p.Color,
            MainImageUrl = p.MainImageUrl,
            Id = p.Id
        });
    }

    public async Task<ProductsDto> GetProductsBySellerIdAsync(Guid sellerId, int offset, int limit)
    {
        var products = await _repository.GetProductsBySellerIdAsync(sellerId, offset, limit);
        var info = await _repository.GetPaginationInfoAsync(offset, limit, null);
        return new ProductsDto
        {
            Products = products.Select(p => p.ToDto()).ToList(),
            Pagination = info
        };
    }

    public async Task<bool> ChangeProductStateAsync(Guid id, string state)
    {
        var product = await _repository.GetProductByIdAsync(id);
        if (product == null)
        {
            return false;
        }

        product.State = state;
        await _repository.UpdateProductAsync(product);
        return true;
    }

    public async Task<bool> ChangeProductQuantityAsync(Guid id, string operation, int quantity)
    {
        if (quantity < 0)
        {
            return false; // Invalid quantity
        }

        var product = await _repository.GetProductByIdAsync(id);
        if (product == null)
        {
            return false;
        }

        switch (operation)
        {
            case "add":
                product.Quantity += (uint)quantity;
                break;
            case "minus":
                var newQuantity = (int)product.Quantity - quantity;
                product.Quantity = (uint)Math.Max(0, newQuantity); // Prevent negative quantity
                break;
            case "set":
                product.Quantity = (uint)quantity;
                break;
            default:
                return false;
        }

        await _repository.UpdateProductAsync(product);
        return true;
    }

    public async Task<bool> UpdateProductAsync(Guid id, Product product, IEnumerable<UpdateCharacteristicDto>? characteristics = null)
    {
        // Get the existing product to preserve certain fields and update characteristics
        var existingProduct = _repository.GetProductById(id);
        if (existingProduct == null)
        {
            return false;
        }

        // Update the existing product with new values
        existingProduct.Title = product.Title;
        existingProduct.Producer = product.Producer;
        existingProduct.IsNew = product.IsNew;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.Color = product.Color;
        existingProduct.Weight = product.Weight;
        existingProduct.MainImageUrl = product.MainImageUrl;
        existingProduct.CategoryId = product.CategoryId;
        existingProduct.Quantity = product.Quantity;
        existingProduct.State = product.State; // Set state to moderation when updated

        // Update characteristics
        if (characteristics != null)
        {
            foreach (var characteristic in characteristics)
            {
                var existing = existingProduct.Characteristics.FirstOrDefault(c => c.Id == characteristic.Id);
                if (existing != null)
                {
                    existing.Value = characteristic.Value;
                }
                else
                {
                    existingProduct.Characteristics.Add(new Characteristic
                    {
                        ProductId = existingProduct.Id,
                        Value = characteristic.Value
                    });
                }
            }
        }

        await _repository.UpdateProductAsync(existingProduct);
        return true;
    }

    // Новий метод для оновлення продукту з файлами для веб-інтерфейсу
    public async Task<Product?> UpdateProductWithFilesAsync(Guid id, UpdateProductWithFilesDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Крок 1: Отримати існуючий продукт
            var existingProduct = _repository.GetProductById(id);
            if (existingProduct == null)
            {
                return null;
            }

            // Крок 2: Перевірити, чи існує категорія
            var category = _categoryRepository.GetCategoryById(updateDto.CategoryId);
            if (category == null)
            {
                throw new ArgumentException($"Category with id {updateDto.CategoryId} does not exist");
            }

            // Крок 3: Оновити основні поля продукту
            existingProduct.Title = updateDto.Title;
            existingProduct.Producer = updateDto.Producer;
            existingProduct.IsNew = updateDto.IsNew;
            existingProduct.Description = updateDto.Description;
            existingProduct.Price = updateDto.Price;
            existingProduct.Color = updateDto.Color;
            existingProduct.Weight = updateDto.Weight;
            existingProduct.CategoryId = updateDto.CategoryId;
            existingProduct.Quantity = updateDto.Quantity;
            existingProduct.State = ProductState.Moderation; // Встановлюємо стан на модерацію
            existingProduct.Category = category;

            // Крок 4: Оновити головне зображення, якщо надано нове
            if (updateDto.MainImage != null)
            {
                // Видалити старе головне зображення, якщо воно існує
                if (!string.IsNullOrEmpty(existingProduct.MainImageUrl))
                {
                    await _filesServiceClient.DeleteImageAsync(existingProduct.MainImageUrl, cancellationToken);
                }

                // Завантажити нове головне зображення
                var mainImageFileName = await _filesServiceClient.UploadImageAsync(updateDto.MainImage, cancellationToken);
                existingProduct.MainImageUrl = mainImageFileName;
            }

            // Крок 5: Видалити зазначені зображення
            if (updateDto.ImagesToDelete != null && updateDto.ImagesToDelete.Any())
            {
                foreach (var imageId in updateDto.ImagesToDelete)
                {
                    await _attachmentService.DeleteAttachmentAsync(imageId, cancellationToken);
                }
            }

            // Крок 6: Додати нові додаткові зображення
            if (updateDto.AdditionalImages != null && updateDto.AdditionalImages.Any())
            {
                foreach (var image in updateDto.AdditionalImages)
                {
                    var attachmentDto = new CreateAttachmentDto
                    {
                        File = image,
                        ProductId = existingProduct.Id
                    };
                    await _attachmentService.CreateAttachmentAsync(attachmentDto, cancellationToken);
                }
            }

            // Крок 7: Оновити характеристики
            if (updateDto.Characteristics != null && updateDto.Characteristics.Any())
            {
                foreach (var characteristic in updateDto.Characteristics)
                {
                    var existing = existingProduct.Characteristics.FirstOrDefault(c => c.Id == characteristic.Id);
                    if (existing != null)
                    {
                        existing.Value = characteristic.Value;
                    }
                }
            }

            // Крок 8: Зберегти оновлений продукт
            await _repository.UpdateProductAsync(existingProduct);

            return existingProduct;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product with files for product {ProductId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByStateAsync(string state)
    {
        return await _repository.GetProductsByStateAsync(state);
    }

    public async Task<ProductsDto> GetProductsWithFilterAsync(int offset, int limit, Guid? sellerId = null, Guid? categoryId = null, string? status = null)
    {
        var products = await _repository.GetProductsWithFilterAsync(offset, limit, sellerId, categoryId, status);
        var info = await _repository.GetPaginationInfoWithFilterAsync(offset, limit, sellerId, categoryId, status);

        var productsDto = new ProductsDto
        {
            Products = products.ToDto()?.ToList() ?? new List<ProductDto>(),
            Pagination = info
        };

        return productsDto;
    }
}