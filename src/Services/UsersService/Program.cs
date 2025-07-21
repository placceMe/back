using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using UsersService.Data;
using UsersService.Repositories;
using UsersService.Services;
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

// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "default-secret-key-for-development-only-min-32-chars";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "UsersService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MarketplaceClient";

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

        // Configure cookie authentication
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Read token from cookie instead of Authorization header
                if (context.Request.Cookies.TryGetValue("authToken", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

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

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users_service")));


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<DatabaseMigrationService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

try
{
    app.Logger.LogInformation("Початок застосування міграцій для Users Service...");

    using var scope = app.Services.CreateScope();
    var migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();
    await migrationService.MigrateDatabaseAsync<UsersDbContext>();

    app.Logger.LogInformation("Міграції для Users Service успішно застосовані");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Помилка при застосуванні міграцій для Users Service: {Message}", ex.Message);

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

// Auth middleware order is important!
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
Log.Information("Test log for PostgreSQL sink");

app.Run();
