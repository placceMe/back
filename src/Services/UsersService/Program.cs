using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UsersService.Data;
using UsersService.Repositories;
using UsersService.Services;
using UsersService.Extensions;
using Serilog;
using Serilog.Sinks.PostgreSQL;

// + NEW:
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System.Security.Cryptography;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// ---- Serilog (як у вас) ----
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        tableName: "Logs",
        needAutoCreateTable: true
    )
    .CreateLogger();
builder.Host.UseSerilog();

// ---- JWT config (як у вас) ----
var jwtKey = builder.Configuration["Jwt:Key"] ?? "default-secret-key-for-development-only-min-32-chars";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "UsersService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MarketplaceClient";

// + NEW: Redis multiplexer (один на застосунок)
var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";
var multiplexer = ConnectionMultiplexer.Connect(redisConn);
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

// + NEW: Data Protection key ring у Redis (спільні ключі між інстансами)
builder.Services
    .AddDataProtection()
    .PersistKeysToStackExchangeRedis(multiplexer, "dp-keys");

// + NEW: IDistributedCache на Redis (для різних сценаріїв)
builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = redisConn;
    o.InstanceName = "userssvc:";
});

// + NEW: Наші сервіси для Redis-аутентифікації
builder.Services.AddSingleton<IRedisAuthStore, RedisAuthStore>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Token з cookie (як у вас)
                if (context.Request.Cookies.TryGetValue("authToken", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },

            // + NEW: Перевірка denylist (revoked jti) у Redis — працює на всіх інстансах
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrWhiteSpace(jti))
                    return;

                var store = context.HttpContext.RequestServices.GetRequiredService<IRedisAuthStore>();
                var isRevoked = await store.IsJtiRevokedAsync(jti, context.HttpContext.RequestAborted);
                if (isRevoked)
                {
                    context.Fail("Token revoked");
                }
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users_service")));

// ---- ваші DI ----
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISalerInfoRepository, SalerInfoRepository>();
builder.Services.AddScoped<ISalerInfoService, SalerInfoService>();
builder.Services.AddScoped<DatabaseMigrationService>();

// HttpClient for NotificationsService
builder.Services.AddHttpClient<INotificationServiceClient, NotificationServiceClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["NotificationsService:BaseUrl"] ?? "http://notifications-service:80/";
    client.BaseAddress = new Uri(baseUrl);
});


builder.Services.AddHealthChecks();

var app = builder.Build();

// ---- міграції (як у вас) ----
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("Test log for PostgreSQL sink");

app.Run();
