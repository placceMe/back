using ChatService.Data;
using ChatService.Hubs;
using ChatService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register ChatDbContext with PostgreSQL
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "chat_service")
    )
);

// HTTP Client for ProductsService
builder.Services.AddHttpClient<ProductsServiceClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["ProductsService:BaseUrl"] ?? "http://localhost:5003/");
});

// HTTP Client for UsersService
builder.Services.AddHttpClient<UsersServiceClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["UsersService:BaseUrl"] ?? "http://localhost:5002/");
});

// Register NotificationService
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add SignalR with configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // ??? debugging
});

// Add CORS for SignalR with specific origins to support credentials
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // Get allowed origins from configuration
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                           ?? new[] { "http://localhost:5173", "http://localhost:3000" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // This is required for SignalR
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Chat Service API", 
        Version = "v1",
        Description = "API ??? ?????????? ?????? ??? ????????? ?? ?????????? ?? ???????"
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Apply EF Core migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat Service API v1");
    c.RoutePrefix = "swagger";
});

// Serve static files (for SignalR test page)
app.UseStaticFiles();

// CORS must be before routing and authorization
app.UseCors();

app.UseRouting(); // Explicit routing

app.UseAuthorization();

app.MapControllers();

// Map SignalR hub
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
