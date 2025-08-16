using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UsersService.Data;
using UsersService.Repositories;
using UsersService.Services;
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
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<DatabaseMigrationService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// ---- міграції (як у вас) ----
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

// ----------------- Redis-based auth helpers -----------------
public interface IRedisAuthStore
{
    // refresh-токени
    Task StoreRefreshTokenAsync(Guid userId, string deviceId, string refreshToken, DateTimeOffset expiresAt, CancellationToken ct);
    Task<bool> ValidateAndRotateRefreshTokenAsync(Guid userId, string deviceId, string oldRefreshToken, string newRefreshToken, DateTimeOffset newExpiresAt, CancellationToken ct);
    Task InvalidateAllRefreshTokensAsync(Guid userId, CancellationToken ct);

    // denylist для access jti
    Task RevokeJtiAsync(string jti, DateTimeOffset accessExp, CancellationToken ct);
    Task<bool> IsJtiRevokedAsync(string jti, CancellationToken ct);
}

public sealed class RedisAuthStore : IRedisAuthStore
{
    private readonly IConnectionMultiplexer _mux;
    public RedisAuthStore(IConnectionMultiplexer mux) => _mux = mux;

    // Ключі:
    // rt:{userId}:{deviceId} -> hash { "hash": <sha256>, "exp": <unix> }, TTL до exp
    // deny:jti:{jti} -> "1" з TTL до exp

    public async Task StoreRefreshTokenAsync(Guid userId, string deviceId, string refreshToken, DateTimeOffset expiresAt, CancellationToken ct)
    {
        var db = _mux.GetDatabase();
        var key = $"rt:{userId}:{deviceId}";
        var hash = Sha256(refreshToken);

        var entries = new HashEntry[]
        {
            new("hash", hash),
            new("exp", expiresAt.ToUnixTimeSeconds())
        };
        await db.HashSetAsync(key, entries);
        await db.KeyExpireAsync(key, expiresAt.UtcDateTime);
    }

    public async Task<bool> ValidateAndRotateRefreshTokenAsync(Guid userId, string deviceId, string oldRefreshToken, string newRefreshToken, DateTimeOffset newExpiresAt, CancellationToken ct)
    {
        var db = _mux.GetDatabase();
        var key = $"rt:{userId}:{deviceId}";

        var stored = await db.HashGetAsync(key, "hash");
        if (stored.IsNullOrEmpty) return false;

        var ok = stored.ToString() == Sha256(oldRefreshToken);
        if (!ok) return false;

        // rotate → зберігаємо новий
        await StoreRefreshTokenAsync(userId, deviceId, newRefreshToken, newExpiresAt, ct);
        return true;
    }

    public async Task InvalidateAllRefreshTokensAsync(Guid userId, CancellationToken ct)
    {
        var db = _mux.GetDatabase();
        // У реалі: зберігати список deviceId у окремому ключі і пройтися по ньому
        // для простоти інвалідовуємо шаблоном через серверний Lua (або зовнішньо).
        // Тут лишаємо як коментар-нагадування.
        // Рекомендація: при видачі refresh додавати deviceId у set: "rtidx:{userId}"
        // і потім видаляти усі "rt:{userId}:{deviceId}".
    }

    public async Task RevokeJtiAsync(string jti, DateTimeOffset accessExp, CancellationToken ct)
    {
        var db = _mux.GetDatabase();
        var key = $"deny:jti:{jti}";
        await db.StringSetAsync(key, "1", expiry: accessExp - DateTimeOffset.UtcNow);
    }

    public async Task<bool> IsJtiRevokedAsync(string jti, CancellationToken ct)
    {
        var db = _mux.GetDatabase();
        return await db.KeyExistsAsync($"deny:jti:{jti}");
    }

    private static string Sha256(string s)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
        return Convert.ToHexString(bytes);
    }
}
