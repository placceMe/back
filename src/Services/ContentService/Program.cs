using ContentService.Data;
using ContentService.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.PostgreSQL(connectionString, "Logs", needAutoCreateTable: true)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddDbContext<ContentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "content_service")));

builder.Services.AddControllers();

// CORS configuration
var allowedOrigins = builder.Configuration["ALLOWED_ORIGINS"]?.Split(';') ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Authentication - будемо додавати пізніше
// builder.Services.AddAuthentication();
// builder.Services.AddMarketplaceAuthentication(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application services
builder.Services.RegisterApplicationServices();

// HTTP Clients
builder.Services.AddHttpClient<ContentService.Services.FilesServiceClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var filesServiceUrl = configuration["FilesService:BaseUrl"] ?? "http://files-service:80/";
    Log.Information($"Configuring FilesServiceClient with BaseUrl: {filesServiceUrl}");
    client.BaseAddress = new Uri(filesServiceUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddScoped<ContentService.Services.Interfaces.IFilesServiceClient, ContentService.Services.FilesServiceClient>();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Apply migrations
try
{
    app.Logger.LogInformation("Початок застосування міграцій для Content Service...");
    app.ApplyMigrations<ContentDbContext>();
    app.Logger.LogInformation("Міграції для Content Service успішно застосовані");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Помилка при застосуванні міграцій для Content Service: {Message}", ex.Message);
    if (app.Environment.IsProduction())
        throw;
    app.Logger.LogWarning("Продовжуємо роботу в development режимі незважаючи на помилки міграцій");
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting ContentService");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ContentService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
