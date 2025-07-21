# API Gateway Documentation

## Base URL
```
http://localhost:80
```

## Overview

The API Gateway serves as the single entry point for all client requests to the marketplace backend. It uses **NGINX** as a reverse proxy combined with **YARP (Yet Another Reverse Proxy)** for intelligent routing to microservices.

---

## Routing Configuration

### Service Routes

All requests are routed based on path prefixes:

| Path Prefix | Target Service | Internal URL |
|-------------|----------------|--------------|
| `/api/users/` | Users Service | `users-service:80` |
| `/api/products/` | Products Service | `products-service:80` |
| `/api/files/` | Files Service | `files-service:80` |
| `/api/orders/` | Orders Service | `orders-service:8000` |

### Example Routing

```bash
# Client Request → Gateway → Target Service
GET http://localhost:80/api/users/123
# Routes to → users-service:80/api/users/123

GET http://localhost:80/api/products
# Routes to → products-service:80/api/products

POST http://localhost:80/api/files/upload
# Routes to → files-service:80/api/files/upload

GET http://localhost:80/api/orders/user/123
# Routes to → orders-service:8000/api/orders/user/123
```

---

## Load Balancing

The gateway supports load balancing across multiple instances of each service:

```nginx
upstream users_service {
    server users-service:80;
    # Additional instances can be added here
}

upstream products_service {
    server products-service:80;
    # Additional instances can be added here
}
```

---

## Health Checks

### Gateway Health Check
```http
GET /health
```

**Response:**
```json
{
  "status": "healthy",
  "gateway": "nginx-yarp",
  "timestamp": "2024-01-01T12:00:00.000Z"
}
```

### Service Health Checks
The gateway can proxy health check requests to individual services:

```http
GET /api/users/health    # → users-service health
GET /api/products/health # → products-service health
GET /api/files/health    # → files-service health
GET /api/orders/health   # → orders-service health
```

---

## CORS Configuration

The gateway is configured to handle CORS for all services:

```nginx
# CORS headers added at gateway level
add_header 'Access-Control-Allow-Origin' '*';
add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, OPTIONS';
add_header 'Access-Control-Allow-Headers' 'DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range,Authorization';
```

---

## Request/Response Flow

### 1. Client Request
```bash
curl -X GET "http://localhost:80/api/products/123"
```

### 2. Gateway Processing
- NGINX receives the request
- Path `/api/products/` matches routing rule
- Request forwarded to `products-service:80`

### 3. Service Response
- Products Service processes the request
- Response returned through gateway to client

### 4. Response Headers
Gateway adds standard headers:
- CORS headers
- Request ID for tracing
- Cache control headers

---

## Configuration Files

### NGINX Configuration
```nginx
# Location: nginx/nginx.conf
server {
    listen 80;
    
    location /api/users/ {
        proxy_pass http://users-service:80/;
    }
    
    location /api/products/ {
        proxy_pass http://products-service:80/;
    }
    
    location /api/files/ {
        proxy_pass http://files-service:80/;
    }
    
    location /api/orders/ {
        proxy_pass http://orders-service:8000/;
    }
}
```

### YARP Configuration
```json
{
  "Routes": {
    "users-route": {
      "ClusterId": "users-cluster",
      "Match": {
        "Path": "/api/users/{**catch-all}"
      }
    }
  },
  "Clusters": {
    "users-cluster": {
      "Destinations": {
        "users-service": {
          "Address": "http://users-service:80"
        }
      }
    }
  }
}
```

---

## Error Handling

### Gateway-Level Errors

#### 502 Bad Gateway
When target service is unavailable:
```json
{
  "error": "Service temporarily unavailable",
  "service": "products-service",
  "timestamp": "2024-01-01T12:00:00.000Z"
}
```

#### 504 Gateway Timeout
When service doesn't respond in time:
```json
{
  "error": "Service timeout",
  "timeout": "30s",
  "timestamp": "2024-01-01T12:00:00.000Z"
}
```

---

## Monitoring and Logging

### Request Logging
All requests are logged with:
- Request ID
- Source IP
- Target service
- Response time
- Status code

### Metrics Available
- Requests per second by service
- Response times
- Error rates
- Service availability

---

## Security Features

### Request Limits
```nginx
# Rate limiting per IP
limit_req_zone $binary_remote_addr zone=api:10m rate=100r/m;

# Request size limits
client_max_body_size 50M;
```

### Headers Security
```nginx
# Security headers
add_header X-Frame-Options "SAMEORIGIN";
add_header X-Content-Type-Options "nosniff";
add_header X-XSS-Protection "1; mode=block";
```

---

## Development vs Production

### Development Configuration
- Allow all CORS origins
- Detailed error messages
- Extended timeouts

### Production Configuration (Recommended)
- Restrict CORS origins
- Hide internal error details
- Implement authentication middleware
- Enable SSL/TLS termination

---

## Usage Examples

### API Discovery
```bash
# Get all available services
curl -X GET "http://localhost:80/api/discovery"
```

### Service-Specific Requests
```bash
# Users
curl -X GET "http://localhost:80/api/users"

# Products
curl -X GET "http://localhost:80/api/products"

# Files
curl -X POST "http://localhost:80/api/files/upload" \
  -F "file=@image.jpg"

# Orders
curl -X GET "http://localhost:80/api/orders/user/123"
```

---

## Notes

1. **Single Entry Point**: All client applications should use port 80
2. **Service Discovery**: Gateway handles routing, clients don't need service-specific ports
3. **Development**: Services are also accessible directly on their specific ports for debugging
4. **Scalability**: Gateway can be scaled horizontally with load balancer
5. **SSL**: In production, configure SSL termination at gateway level