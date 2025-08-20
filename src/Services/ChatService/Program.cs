using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ChatService.Data;
using ChatService.Hubs;
using ChatService.Services;
using ChatService.Repositories;
using ChatService.Extensions;
using ChatService.HttpClients;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "chat_service")));

// SignalR
builder.Services.AddSignalR();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default-key-for-development"))
        };

        // Для SignalR підтримки токенів через query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// CORS
var allowedOrigins = builder.Configuration["ALLOWED_ORIGINS"]?.Split(',') ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// HttpClients для комунікації з іншими сервісами - following project pattern
builder.Services.AddHttpClient<IUsersServiceClient, UsersServiceClient>((serviceProvider, client) =>
{
    client.BaseAddress = new Uri(builder.Configuration["UsersService:BaseUrl"] ?? "http://users-service:80/");
});

builder.Services.AddHttpClient<IProductsServiceClient, ProductsServiceClient>((serviceProvider, client) =>
{
    client.BaseAddress = new Uri(builder.Configuration["ProductsService:BaseUrl"] ?? "http://products-service:80/");
});

// Services - following project conventions
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatServiceImplementation>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

// Apply migrations
app.ApplyMigrations<ChatDbContext>();

app.Run();
