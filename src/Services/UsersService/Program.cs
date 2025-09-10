using Microsoft.EntityFrameworkCore;
using UsersService.Data;
using UsersService.Repositories;
using UsersService.Services;
using UsersService.Extensions;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ---- Serilog ----
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        tableName: "Logs",
        needAutoCreateTable: true
    )
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users_service")));

// ---- Redis ----
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Redis") ??
                          configuration["Redis:DefaultConnection"] ??
                          "localhost:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});

// ---- Service DI ----
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISalerInfoRepository, SalerInfoRepository>();
builder.Services.AddScoped<ISalerInfoService, SalerInfoService>();
builder.Services.AddScoped<IRedisAuthStore, RedisAuthStore>();
builder.Services.AddScoped<DatabaseMigrationService>();

// ---- Authentication ----
// Add the shared authentication library
// builder.Services.AddMarketplaceAuthentication(builder.Configuration, "UsersService");

// Temporary manual authentication setup until shared library reference is resolved
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Authentication:Jwt");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "default-key-32-chars-minimum-length")),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        // Handle token from cookie
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Try to get token from cookie first
                if (context.Request.Cookies.TryGetValue("authToken", out var cookieToken))
                {
                    context.Token = cookieToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ---- CORS ----
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Frontend URLs
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for cookies
    });
});

// HttpClient for NotificationsService
builder.Services.AddHttpClient<INotificationServiceClient, NotificationServiceClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["NotificationsService:BaseUrl"] ?? "http://notifications-service:8080/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// ---- Migrations ----
try
{
    app.Logger.LogInformation("Початок застосування міграцій для Users Service...");
    app.ApplyMigrations<UsersDbContext>();
    app.Logger.LogInformation("Міграції для Users Service успішно застосовані");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Помилка при застосуванні міграцій для Users Service: {Message}", ex.Message);
    if (app.Environment.IsProduction())
        throw;
    app.Logger.LogWarning("Продовжуємо роботу в development режимі незважаючи на помилки міграцій");
}

app.UseSwagger();
app.UseSwaggerUI();

// ---- CORS ----
app.UseCors();

// ---- Authentication & Authorization ----
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("Users Service запущено");

app.Run();
