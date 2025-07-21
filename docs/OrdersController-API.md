# Orders Service API Documentation

## Base URL
```
http://localhost:5005/api/orders
```

## Endpoints

### 1. Create Order
**POST** `/api/orders`

Creates a new order in the system.

#### Request Body
```json
{
  "userId": "guid",
  "items": [
    {
      "productId": "guid",
      "quantity": "integer",
      "price": "decimal"
    }
  ],
  "totalAmount": "decimal",
  "shippingAddress": "string"
}
```

#### Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| userId | GUID | Yes | ID of the user placing the order |
| items | Array | Yes | List of order items |
| items[].productId | GUID | Yes | ID of the product being ordered |
| items[].quantity | Integer | Yes | Quantity of the product |
| items[].price | Decimal | Yes | Price per unit of the product |
| totalAmount | Decimal | Yes | Total amount for the order |
| shippingAddress | String | Yes | Shipping address for the order |

#### Response
**Status Code:** `201 Created`
```json
{
  "id": "guid",
  "userId": "guid",
  "items": [...],
  "totalAmount": "decimal",
  "status": "string",
  "createdAt": "datetime",
  "shippingAddress": "string"
}
```

#### Error Responses
- **400 Bad Request**: Invalid request data
- **500 Internal Server Error**: Server error

---

### 2. Get Order by ID
**GET** `/api/orders/{id}`

Retrieves a specific order by its ID.

#### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | GUID | Yes | Unique identifier of the order |

#### Response
**Status Code:** `200 OK`
```json
{
  "id": "guid",
  "userId": "guid",
  "items": [...],
  "totalAmount": "decimal",
  "status": "string",
  "createdAt": "datetime",
  "shippingAddress": "string"
}
```

#### Error Responses
- **404 Not Found**: Order with specified ID not found

---

### 3. Get Orders by User
**GET** `/api/orders/user/{userId}`

Retrieves all orders for a specific user.

#### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| userId | GUID | Yes | Unique identifier of the user |

#### Response
**Status Code:** `200 OK`
```json
[
  {
    "id": "guid",
    "userId": "guid",
    "items": [...],
    "totalAmount": "decimal",
    "status": "string",
    "createdAt": "datetime",
    "shippingAddress": "string"
  }
]
```

---

### 4. Get All Orders
**GET** `/api/orders`

Retrieves all orders in the system.

#### Response
**Status Code:** `200 OK`
```json
[
  {
    "id": "guid",
    "userId": "guid",
    "items": [...],
    "totalAmount": "decimal",
    "status": "string",
    "createdAt": "datetime",
    "shippingAddress": "string"
  }
]
```

---

### 5. Update Order Status
**PUT** `/api/orders/{id}/status`

Updates the status of an existing order.

#### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | GUID | Yes | Unique identifier of the order |

#### Request Body
```json
{
  "status": "string"
}
```

#### Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| status | String | Yes | New status for the order (e.g., "Pending", "Processing", "Shipped", "Delivered", "Cancelled") |

#### Response
**Status Code:** `200 OK`
```json
{
  "id": "guid",
  "userId": "guid",
  "items": [...],
  "totalAmount": "decimal",
  "status": "string",
  "createdAt": "datetime",
  "shippingAddress": "string"
}
```

#### Error Responses
- **404 Not Found**: Order with specified ID not found

---

### 6. Delete Order
**DELETE** `/api/orders/{id}`

Deletes an order from the system.

#### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | GUID | Yes | Unique identifier of the order |

#### Response
**Status Code:** `204 No Content`

#### Error Responses
- **404 Not Found**: Order with specified ID not found

---

## Health Check Endpoint

### Health Check
**GET** `/health`

Returns the health status of the Orders Service.

#### Response
**Status Code:** `200 OK`
```json
{
  "status": "healthy",
  "service": "OrdersServiceNet",
  "timestamp": "2024-01-01T12:00:00.000Z"
}
```

---

## Data Models

### OrderResponse
```json
{
  "id": "guid",
  "userId": "guid",
  "items": [
    {
      "productId": "guid",
      "quantity": "integer",
      "price": "decimal"
    }
  ],
  "totalAmount": "decimal",
  "status": "string",
  "createdAt": "datetime",
  "shippingAddress": "string"
}
```

### CreateOrderRequest
```json
{
  "userId": "guid",
  "items": [
    {
      "productId": "guid",
      "quantity": "integer",
      "price": "decimal"
    }
  ],
  "totalAmount": "decimal",
  "shippingAddress": "string"
}
```

### UpdateOrderStatusRequest
```json
{
  "status": "string"
}
```

---

## Error Handling

All endpoints return appropriate HTTP status codes and error messages:

- **400 Bad Request**: When request validation fails
- **404 Not Found**: When requested resource doesn't exist
- **500 Internal Server Error**: When an unexpected server error occurs

Error responses follow this format:
```json
{
  "message": "Error description"
}
```

---

## Notes

- All datetime values are in UTC format
- GUID values should be in standard GUID format (e.g., "123e4567-e89b-12d3-a456-426614174000")
- The service uses standard HTTP status codes for responses
- Authentication and authorization may be required depending on deployment configuration