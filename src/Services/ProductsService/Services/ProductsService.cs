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
    private readonly IAttachmentService _attachmentService;
    private readonly ICharacteristicService _characteristicService;
    private readonly ILogger<ProductsService> _logger;

    public ProductsService(
        IProductsRepository repository,
        ICategoryRepository categoryRepository,
        IFilesServiceClient filesServiceClient,
        IAttachmentService attachmentService,
        ICharacteristicService characteristicService,
        ILogger<ProductsService> logger)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _filesServiceClient = filesServiceClient;
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
                Category = category
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

    public bool UpdateProduct(Guid id, Product product)
    {
        return _repository.UpdateProduct(id, product);
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
            Products = (List<ProductDto>)(products.ToDto() ?? new List<ProductDto>()),
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
            Products = (List<ProductDto>)(products.ToDto() ?? new List<ProductDto>()),
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
            MainImageUrl = p.MainImageUrl
        });
    }
}