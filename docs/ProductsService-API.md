# Products Service API Documentation

## Base URL
```
http://localhost:5003/api
```

## Products Controller

### 1. Get All Products
```http
GET /api/products
```

**Response:**
```json
[
  {
    "id": "uuid",
    "title": "string",
    "description": "string",
    "price": "decimal",
    "color": "string",
    "weight": "decimal",
    "mainImageUrl": "string",
    "categoryId": "uuid",
    "sellerId": "uuid",
    "quantity": "int"
  }
]
```

### 2. Get Product by ID
```http
GET /api/products/{id}
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | UUID | Yes | Unique identifier of the product |

**Response:**
```json
{
  "id": "uuid",
  "title": "string",
  "description": "string",
  "price": "decimal",
  "color": "string",
  "weight": "decimal",
  "mainImageUrl": "string",
  "categoryId": "uuid",
  "sellerId": "uuid",
  "quantity": "int"
}
```

### 3. Create Product with Files
```http
POST /api/products/with-files
Content-Type: multipart/form-data
```

**Request Body (FormData):**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| title | string | Yes | Product title |
| description | string | Yes | Product description |
| price | decimal | Yes | Product price |
| color | string | Yes | Product color |
| weight | decimal | Yes | Product weight |
| categoryId | uuid | Yes | Category identifier |
| sellerId | uuid | Yes | Seller identifier |
| quantity | int | Yes | Available quantity |
| files | File[] | No | Product images |

**Response:**
```json
{
  "id": "uuid",
  "title": "string",
  "description": "string",
  "price": "decimal",
  "color": "string",
  "weight": "decimal",
  "mainImageUrl": "string",
  "categoryId": "uuid",
  "sellerId": "uuid",
  "quantity": "int"
}
```

### 4. Create Product (without files)
```http
POST /api/products
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "string",
  "description": "string",
  "price": "decimal",
  "color": "string",
  "weight": "decimal",
  "mainImageUrl": "string",
  "categoryId": "uuid",
  "sellerId": "uuid",
  "quantity": "int"
}
```

**Response:**
```json
{
  "id": "uuid",
  "title": "string",
  "description": "string",
  "price": "decimal",
  "color": "string",
  "weight": "decimal",
  "mainImageUrl": "string",
  "categoryId": "uuid",
  "sellerId": "uuid",
  "quantity": "int"
}
```

### 5. Update Product
```http
PUT /api/products/{id}
Content-Type: application/json
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | UUID | Yes | Unique identifier of the product |

**Request Body:**
```json
{
  "title": "string",
  "description": "string",
  "price": "decimal",
  "color": "string",
  "weight": "decimal",
  "mainImageUrl": "string",
  "categoryId": "uuid",
  "quantity": "int"
}
```

**Response:**
```json
{
  "id": "uuid",
  "title": "string",
  "description": "string",
  "price": "decimal",
  "color": "string",
  "weight": "decimal",
  "mainImageUrl": "string",
  "categoryId": "uuid",
  "sellerId": "uuid",
  "quantity": "int"
}
```

### 6. Delete Product
```http
DELETE /api/products/{id}
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | UUID | Yes | Unique identifier of the product |

**Response:**
- **204 No Content**: Product deleted successfully
- **404 Not Found**: Product not found

---

## Attachment Controller

### 1. Get Attachment by ID
```http
GET /api/attachment/{id}
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | UUID | Yes | Unique identifier of the attachment |

**Response:**
```json
{
  "id": "uuid",
  "fileName": "string",
  "fileUrl": "string",
  "productId": "uuid",
  "createdAt": "datetime"
}
```

### 2. Get Attachment File
```http
GET /api/attachment/{id}/file
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | UUID | Yes | Unique identifier of the attachment |

**Response:** Binary file data

### 3. Get Attachments by Product
```http
GET /api/attachment/product/{productId}
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| productId | UUID | Yes | Unique identifier of the product |

**Response:**
```json
[
  {
    "id": "uuid",
    "fileName": "string",
    "fileUrl": "string",
    "productId": "uuid",
    "createdAt": "datetime"
  }
]
```

### 4. Create Attachment
```http
POST /api/attachment
Content-Type: multipart/form-data
```

**Request Body (FormData):**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| file | File | Yes | File to upload |
| productId | UUID | Yes | Product identifier |

**Response:**
```json
{
  "id": "uuid",
  "fileName": "string",
  "fileUrl": "string",
  "productId": "uuid",
  "createdAt": "datetime"
}
```

### 5. Delete Attachment
```http
DELETE /api/attachment/{id}
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | UUID | Yes | Unique identifier of the attachment |

**Response:**
- **204 No Content**: Attachment deleted successfully
- **404 Not Found**: Attachment not found

---

## Status Codes

### Success Responses:
- **200 OK** - Request completed successfully
- **201 Created** - Resource created successfully
- **204 No Content** - Request completed successfully, no content returned

### Client Errors:
- **400 Bad Request** - Invalid request
- **404 Not Found** - Resource not found

### Server Errors:
- **500 Internal Server Error** - Internal server error

---

## Notes

1. File uploads should use `Content-Type: multipart/form-data`
2. JSON requests should use `Content-Type: application/json`
3. All UUIDs should be in standard format
4. File size limits apply as configured on the server