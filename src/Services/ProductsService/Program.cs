using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL; // Add this line
using ProductsService.Data;
using ProductsService.Repositories;
using ProductsService.Services;
using ProductsService.Repositories.Interfaces;
using ProductsService.Services.Interfaces;
using ProductsService.Models;
using Serilog;
using Serilog.Sinks.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
// Log.Logger = new LoggerConfiguration()
//     .ReadFrom.Configuration(builder.Configuration)
//     .CreateLogger();
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        tableName: "Logs",
        needAutoCreateTable: true
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Remove the alternative configuration for now
builder.Services.AddHttpClient<FilesServiceClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var filesServiceUrl = configuration["FilesService:BaseUrl"] ?? "http://files-service:80/";
    Log.Information($"Configuring FilesServiceClient with BaseUrl: {filesServiceUrl}");
    client.BaseAddress = new Uri(filesServiceUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<ProductsDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "products_service");
    }));

builder.Services.AddScoped<IProductsService, ProductsService.Services.ProductsService>();
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICharacteristicDictService, CharacteristicDictService>();
builder.Services.AddScoped<ICharacteristicDictRepository, CharacteristicDictRepository>();
builder.Services.AddScoped<ICharacteristicService, CharacteristicService>();
builder.Services.AddScoped<ICharacteristicRepository, CharacteristicRepository>();

// Repository and Service registration
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IFilesServiceClient, FilesServiceClient>();
builder.Services.AddScoped<DatabaseMigrationService>();



builder.Services.AddHealthChecks();

var app = builder.Build();

// Apply database migrations automatically
try
{
    app.Logger.LogInformation("Початок застосування міграцій для Products Service...");

    using var scope = app.Services.CreateScope();
    var migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();
    await migrationService.MigrateDatabaseAsync<ProductsDBContext>();

    app.Logger.LogInformation("Міграції для Products Service успішно застосовані");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Помилка при застосуванні міграцій для Products Service: {Message}", ex.Message);

    // В development можемо продовжити роботу, в production - краще зупинити
    if (app.Environment.IsProduction())
    {
        throw;
    }

    app.Logger.LogWarning("Продовжуємо роботу в development режимі незважаючи на помилки міграцій");
}

app.UseSwagger();
app.UseSwaggerUI();

// Enable CORS
app.UseCors();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
Log.Information("Test log for PostgreSQL sink");

app.Run();