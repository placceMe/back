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
| notes | String | No | Optional notes for the order |
| deliveryAddress | String | No | Delivery address for the order |
| items | Array | Yes | List of order items |
| items[].productId | GUID | Yes | ID of the product being ordered |
| items[].quantity | Integer | Yes | Quantity of the product |

#### Response
**Status Code:** `201 Created` or `200 OK`

**Single Seller Response:**
```json
{
  "orders": [
    {
      "id": "guid",
      "userId": "guid",
      "items": [...],
      "totalAmount": "decimal",
      "status": "string",
      "createdAt": "datetime",
      "deliveryAddress": "string"
    }
  ],
  "totalAmount": "decimal",
  "ordersCount": 1,
  "message": "Замовлення успішно створено."
}
```

**Multiple Sellers Response:**
```json
{
  "orders": [
    {
      "id": "guid1",
      "userId": "guid",
      "items": [...],
      "totalAmount": "decimal1",
      "status": "string",
      "createdAt": "datetime",
      "deliveryAddress": "string"
    },
    {
      "id": "guid2",
      "userId": "guid",
      "items": [...],
      "totalAmount": "decimal2",
      "status": "string",
      "createdAt": "datetime",
      "deliveryAddress": "string"
    }
  ],
  "totalAmount": "decimal_total",
  "ordersCount": 2,
  "message": "Ваші товари від 2 різних продавців були розділені на окремі замовлення."
}
```

**Note:** If the order contains products from multiple sellers, the system will automatically create separate orders for each seller while maintaining the same delivery address and notes.

#### Error Responses
- **400 Bad Request**: Invalid request data
- **403 Forbidden**: User can only create orders for themselves
- **404 Not Found**: User or product not found
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

### 4. Get Orders by Seller
**GET** `/api/orders/by-seller/{id}`

Retrieves all orders that contain products sold by a specific seller.

#### Path Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | GUID | Yes | Unique identifier of the seller |

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

#### Authorization
- Requires valid JWT token
- Sellers can only view their own orders
- Admins can view any seller's orders

---

### 5. Get All Orders
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

### 6. Update Order Status
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

### 7. Delete Order
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