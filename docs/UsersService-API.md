# Users Service API Documentation

## Base URL
```
http://localhost:5002/api
```

## Overview

The Users Service handles user management, authentication, and profile operations in the marketplace backend. This service is built with C# .NET 8 and uses PostgreSQL with the `users_service` schema.

---

## Authentication

*Note: Authentication endpoints and implementation details should be added here based on the actual UsersService implementation.*

---

## User Management Endpoints

*Note: This section will be populated with actual endpoint documentation from the UsersService once the controller implementation is available.*

### Typical User Service Endpoints (Expected):

#### 1. Register User
```http
POST /api/users/register
Content-Type: application/json
```

#### 2. Login User
```http
POST /api/users/login
Content-Type: application/json
```

#### 3. Get User Profile
```http
GET /api/users/{id}
```

#### 4. Update User Profile
```http
PUT /api/users/{id}
Content-Type: application/json
```

#### 5. Delete User
```http
DELETE /api/users/{id}
```

#### 6. Get All Users (Admin)
```http
GET /api/users
```

---

## Health Check

### Health Check
```http
GET /health
```

Returns the health status of the Users Service.

**Response:**
```json
{
  "status": "healthy",
  "service": "UsersService",
  "timestamp": "2024-01-01T12:00:00.000Z"
}
```

---

## Status Codes

### Success Responses:
- **200 OK** - Request completed successfully
- **201 Created** - User created successfully
- **204 No Content** - Request completed successfully, no content returned

### Client Errors:
- **400 Bad Request** - Invalid request data
- **401 Unauthorized** - Authentication required
- **403 Forbidden** - Access denied
- **404 Not Found** - User not found
- **409 Conflict** - User already exists

### Server Errors:
- **500 Internal Server Error** - Internal server error

---

## Data Models

### User (Expected Structure)
```json
{
  "id": "uuid",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "phoneNumber": "string",
  "address": "string",
  "role": "string",
  "isActive": "boolean",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

---

## Notes

1. **Database Schema**: Uses PostgreSQL with `users_service` schema isolation
2. **Logging**: Implements Serilog with PostgreSQL sink for centralized logging
3. **CORS**: Configured to allow cross-origin requests for development
4. **Dependency Injection**: Follows standard .NET DI patterns with scoped services

---

## Integration Points

### With Other Services:
- **ProductsService**: User ID referenced in product seller field
- **OrdersService**: User ID referenced in order user field
- **ApiGateway**: Routes `/api/users/` requests to this service

---

*This documentation will be updated as the UsersService implementation is completed.*