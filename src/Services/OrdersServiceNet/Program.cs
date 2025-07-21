using Microsoft.EntityFrameworkCore;
using OrdersServiceNet.Data;
using OrdersServiceNet.Repositories;
using OrdersServiceNet.Services;
using Serilog;
using OrdersServiceNet.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Database context
builder.Services.AddDbContext<OrdersDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "orders_service"));
});

// HTTP clients for inter-service communication
builder.Services.AddHttpClient<ProductsServiceClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["ProductsService:BaseUrl"] ?? "http://localhost:5003/");
});

builder.Services.AddHttpClient<UsersServiceClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["UsersService:BaseUrl"] ?? "http://localhost:5002/");
});

builder.Services.AddScoped<DatabaseMigrationService>();


// Repository pattern
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();

// Business services
builder.Services.AddScoped<IOrderService, OrderService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


try
{
    app.Logger.LogInformation("Початок застосування міграцій для Orders Service...");

    using var scope = app.Services.CreateScope();
    var migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();
    await migrationService.MigrateDatabaseAsync<OrdersDbContext>();

    app.Logger.LogInformation("Міграції для Orders Service успішно застосовані");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Помилка при застосуванні міграцій для Orders Service: {Message}", ex.Message);

    // В development можемо продовжити роботу, в production - краще зупинити
    if (app.Environment.IsProduction())
    {
        throw;
    }

    app.Logger.LogWarning("Продовжуємо роботу в development режимі незважаючи на помилки міграцій");
}
// Apply migrations
//app.ApplyMigrations<OrdersDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRouting();
app.MapControllers();

app.Run();