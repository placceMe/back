# Marketplace Backend - API Documentation Index

## Overview

This documentation covers all microservices in the marketplace backend system. The system uses a hybrid tech stack with C# .NET 8 and Python FastAPI services, PostgreSQL database with schema isolation, and NGINX + YARP for API gateway functionality.

---

## Architecture

### Technology Stack
- **C# .NET 8**: UsersService, ProductsService, FilesService, ApiGateway
- **Python FastAPI**: OrdersService
- **Database**: PostgreSQL with schema-per-service isolation
- **Gateway**: NGINX + YARP for request routing
- **Storage**: MinIO for file storage
- **Logging**: Serilog with PostgreSQL sink

### Service Ports (Development)
| Service | Port | Technology |
|---------|------|------------|
| API Gateway (NGINX) | 80 | NGINX + YARP |
| Users Service | 5002 | C# .NET 8 |
| Products Service | 5003 | C# .NET 8 |
| Files Service | 5001 | C# .NET 8 |
| Orders Service | 5004/5005 | Python FastAPI / C# .NET 8 |

---

## API Documentation by Service

### 🚪 [API Gateway](./ApiGateway-Documentation.md)
**Base URL**: `http://localhost:80`
- Request routing and load balancing
- CORS configuration
- Health checks and monitoring
- Security features

### 👥 [Users Service](./UsersService-API.md)
**Base URL**: `http://localhost:5002/api`
- User registration and authentication
- Profile management
- User roles and permissions

### 📦 [Products Service](./ProductsService-API.md)
**Base URL**: `http://localhost:5003/api`
- Product CRUD operations
- Product attachments management
- Category and seller association

### 📁 [Files Service](./FilesService-API.md)
**Base URL**: `http://localhost:5001/api`
- Image upload and storage
- File retrieval and management
- MinIO integration

### 🛒 [Orders Service](./OrdersController-API.md)
**Base URL**: `http://localhost:5004/api` (Python) / `http://localhost:5005/api` (C#)
- Order creation and management
- Order status updates
- User order history

---

## Quick Start

### Using API Gateway (Recommended)
All services are accessible through the API Gateway at `http://localhost:80`:

```bash
# Products
GET http://localhost:80/api/products

# Users
GET http://localhost:80/api/users

# Files
POST http://localhost:80/api/files/upload

# Orders
GET http://localhost:80/api/orders
```

### Direct Service Access (Development)
Services can also be accessed directly:

```bash
# Products Service
GET http://localhost:5003/api/products

# Files Service
POST http://localhost:5001/api/files/upload

# Orders Service
GET http://localhost:5004/api/orders  # Python FastAPI
GET http://localhost:5005/api/orders  # C# .NET (if available)
```

---

## Common Response Codes

### Success Responses
- **200 OK** - Request completed successfully
- **201 Created** - Resource created successfully
- **204 No Content** - Request completed, no content returned

### Client Errors
- **400 Bad Request** - Invalid request data
- **401 Unauthorized** - Authentication required
- **403 Forbidden** - Access denied
- **404 Not Found** - Resource not found
- **409 Conflict** - Resource conflict

### Server Errors
- **500 Internal Server Error** - Internal server error
- **502 Bad Gateway** - Service unavailable
- **504 Gateway Timeout** - Service timeout

---

## Data Formats

### Request Headers
```http
Content-Type: application/json        # For JSON requests
Content-Type: multipart/form-data     # For file uploads
```

### UUID Format
All IDs use standard UUID format:
```
123e4567-e89b-12d3-a456-426614174000
```

### DateTime Format
All timestamps use ISO 8601 format:
```
2024-01-01T12:00:00.000Z
```

---

## Development Environment

### Starting All Services
```bash
# Start with Docker Compose
docker-compose up -d --build

# Services will be available at their respective ports
```

### Health Checks
```bash
# Check all services through gateway
curl http://localhost:80/api/users/health
curl http://localhost:80/api/products/health
curl http://localhost:80/api/files/health
curl http://localhost:80/api/orders/health
```

---

## Testing

Each service includes `.http` files for API testing:
- `UsersService.http`
- `ProductsService.http`
- `FilesService.http`
- `OrdersService.http`

Use these files with REST client extensions in VS Code or similar tools.

---

## Integration Patterns

### Cross-Service Communication
Services communicate via HTTP with typed clients:

```csharp
// Example: ProductsService → FilesService
builder.Services.AddHttpClient<FilesServiceClient>(client => {
    client.BaseAddress = new Uri("http://files-service:80");
});
```

### File Upload Flow
1. Client uploads product with files to ProductsService
2. ProductsService streams files to FilesService
3. FilesService stores files in MinIO
4. ProductsService saves product with file references

### Database Schema Isolation
Each service uses its own PostgreSQL schema:
- `users_service`
- `products_service` 
- `orders_service`

---

## Notes

1. **Development**: CORS is configured to allow all origins
2. **File Uploads**: Maximum file size limits apply per service configuration
3. **Authentication**: Implementation varies by service (see individual docs)
4. **Error Handling**: All services return consistent error formats
5. **Logging**: Centralized logging with Serilog and PostgreSQL

---

For detailed endpoint documentation, please refer to the individual service documentation files linked above.