# Marketplace Backend - Copilot Instructions

## Architecture Overview

This is a **microservices-based marketplace backend** using a **hybrid tech stack**:

- **C# .NET 8** services: UsersService, ProductsService, FilesService, ApiGateway
- **Python FastAPI** service: OrdersService
- **PostgreSQL** with **schema-per-service** isolation (`users_service`, `products_service`, `orders_service`)
- **NGINX** as reverse proxy + **YARP** in ApiGateway for routing
- **MinIO** for file storage, **Serilog** for PostgreSQL logging

## Service Communication Patterns

### Database Schema Isolation

Each service uses its own PostgreSQL schema:

```bash
# Connection strings include schema isolation
ConnectionStrings__DefaultConnection=Host=postgres-db;Database=marketplace_db;Search Path=users_service;
```

### Inter-Service HTTP Calls

Services communicate via HTTP with typed clients:

```csharp
// ProductsService → FilesService pattern
builder.Services.AddHttpClient<FilesServiceClient>((serviceProvider, client) => {
    client.BaseAddress = new Uri(configuration["FilesService:BaseUrl"]);
});
```

### Shared Contracts

Use `src/Shared/Contracts/` for cross-service DTOs:

```csharp
// Example: CreateProductContract with Stream attachments
public List<Stream> Attachments { get; set; } = new List<Stream>();
```

## Development Workflows

### Local Development

```bash
# Start all services with hot reload
docker-compose up -d --build

# Services available at:
# nginx-gateway: http://localhost:80
# users-service: http://localhost:5002
# products-service: http://localhost:5003
# files-service: http://localhost:5001
# orders-service: http://localhost:5004
```

### Database Migrations

**C# Services**: Use `MigrationExtensions.ApplyMigrations<TContext>()` in Program.cs
**Python Service**: Use Alembic with schema creation in `app/db/create_schema.py`

### Testing Endpoints

Each service has `.http` files for API testing (e.g., `UsersService.http`)

## Project Conventions

### C# Services Structure

```
Controllers/     # API endpoints
Data/           # DbContext
DTOs/           # Data transfer objects
Models/         # Entity models
Repositories/   # Data access layer
Services/       # Business logic
Migrations/     # EF Core migrations
```

### Python Service Structure (OrdersService)

```
app/
  api/v1/       # FastAPI routers
  db/models/    # SQLAlchemy models
  core/         # Configuration
alembic/        # Database migrations
```

### Logging Pattern

All services use Serilog with PostgreSQL sink:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.PostgreSQL(connectionString, "Logs", needAutoCreateTable: true)
    .CreateLogger();
```

### Service Registration Pattern

```csharp
// Standard DI pattern across all C# services
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IService, Service>();
```

## Key Integration Points

### File Upload Flow

1. **ProductsService** receives product with attachments
2. Streams files to **FilesService** via HTTP client
3. **FilesService** stores in **MinIO** and returns URLs
4. Product saved with file references

### NGINX Routing

Routes by path prefix to services:

- `/api/users/` → users-service:80
- `/api/products/` → products-service:80
- `/api/files/` → files-service:80
- `/api/orders/` → orders-service:8000

### CORS Configuration

All services use `AllowAnyOrigin()` for development - update for production

## When Adding New Features

- **New C# service**: Follow UsersService/ProductsService patterns with MigrationExtensions
- **Cross-service calls**: Add typed HttpClient in Program.cs with base URL from config
- **New endpoints**: Add to both nginx.conf routing and service Controllers/
- **Database changes**: Use EF migrations for C#, Alembic for Python
- **File operations**: Always go through FilesService, never direct MinIO access

