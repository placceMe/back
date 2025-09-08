using Microsoft.EntityFrameworkCore;
using OrdersServiceNet.Data;
using OrdersServiceNet.Repositories;
using OrdersServiceNet.Services;
using Serilog;
using OrdersServiceNet.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// 🆕 JWT Authentication Configuration (синхронізовано з UsersService)
// Використовуємо "Key" як в UsersService, а не "SecretKey"
var jwtSecretKey = builder.Configuration["Jwt:Key"] ?? builder.Configuration["Jwt:SecretKey"] ?? "your-super-secret-jwt-key-minimum-32-characters-long-for-security";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "UsersService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MarketplaceClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // 🍪 Support for JWT tokens in cookies and Authorization header
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Check for token in Authorization header first
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                context.Token = authHeader.Substring("Bearer ".Length).Trim();
                return Task.CompletedTask;
            }

            // If no Authorization header, check for token in cookies
            if (context.Request.Cookies.ContainsKey("authToken"))
            {
                context.Token = context.Request.Cookies["authToken"];
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            // 🔍 Додаємо детальне логування помилок аутентифікації
            Log.Warning("JWT Authentication failed: {Exception}", context.Exception?.Message);
            if (context.Exception is SecurityTokenExpiredException)
            {
                Log.Warning("Token expired for request: {Path}", context.Request.Path);
            }
            else if (context.Exception is SecurityTokenInvalidSignatureException)
            {
                Log.Warning("Invalid token signature for request: {Path}", context.Request.Path);
            }
            else if (context.Exception is SecurityTokenInvalidIssuerException)
            {
                Log.Warning("Invalid token issuer for request: {Path}. Expected: {ExpectedIssuer}",
                    context.Request.Path, jwtIssuer);
            }
            else if (context.Exception is SecurityTokenInvalidAudienceException)
            {
                Log.Warning("Invalid token audience for request: {Path}. Expected: {ExpectedAudience}",
                    context.Request.Path, jwtAudience);
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // 🎯 Логування успішної аутентифікації
            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Log.Information("JWT Token validated successfully for user: {UserId} on path: {Path}",
                userId, context.Request.Path);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

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

builder.Services.AddHttpClient<NotificationsServiceClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["NotificationsService:BaseUrl"] ?? "http://localhost:5007/");
});

builder.Services.AddScoped<DatabaseMigrationService>();

// Repository pattern
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();

// Business services
builder.Services.AddScoped<IOrderService, OrderService>();

// CORS (розкоментовано для підтримки frontend запитів з cookies)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') ?? new[] { "http://localhost:5173", "http://localhost:3000" };
        policy.SetIsOriginAllowed(origin => allowedOrigins.Contains(origin))
               .AllowCredentials() // 🔐 Дозволяємо HttpOnly cookies
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OrdersService API", Version = "v1" });

    // 🔐 Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// 🔍 Логування конфігурації JWT для налагодження
Log.Information("OrdersService JWT Configuration:");
Log.Information("- SecretKey length: {KeyLength} characters", jwtSecretKey.Length);
Log.Information("- SecretKey starts with: {KeyStart}", jwtSecretKey.Length > 10 ? jwtSecretKey.Substring(0, 10) + "..." : jwtSecretKey);
Log.Information("- Issuer: {Issuer}", jwtIssuer);
Log.Information("- Audience: {Audience}", jwtAudience);

// Database migrations
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

    if (app.Environment.IsProduction())
    {
        throw;
    }

    app.Logger.LogWarning("Продовжуємо роботу в development режимі незважаючи на помилки міграцій");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRouting();

// 🆕 Add authentication middleware (порядок важливий!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();