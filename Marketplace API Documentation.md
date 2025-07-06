# Marketplace API Documentation

## Базовые URL
- **Users Service**: `http://localhost:5002/api`
- **Products Service**: `http://localhost:5003/api`
- **Files Service**: `http://localhost:5001/api`
- **Gateway (Nginx)**: `http://localhost:80`

---

## Products Service API

### Products Controller

#### 1. Получить все продукты
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

#### 2. Получить продукт по ID
```http
GET /api/products/{id}
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

#### 3. Создать продукт с файлами
```http
POST /api/products/with-files
Content-Type: multipart/form-data
```

**Request Body (FormData):**
```
title: string
description: string
price: decimal
color: string
weight: decimal
categoryId: uuid
sellerId: uuid
quantity: int
files: File[] (изображения)
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

#### 4. Создать продукт (без файлов)
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

#### 5. Обновить продукт
```http
PUT /api/products/{id}
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
  "quantity": "int"
}
```

#### 6. Удалить продукт
```http
DELETE /api/products/{id}
```

---

### Attachment Controller

#### 1. Получить вложение по ID
```http
GET /api/attachment/{id}
```

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

#### 2. Получить файл вложения
```http
GET /api/attachment/{id}/file
```

**Response:** Binary file data

#### 3. Получить вложения по продукту
```http
GET /api/attachment/product/{productId}
```

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

#### 4. Создать вложение
```http
POST /api/attachment
Content-Type: multipart/form-data
```

**Request Body (FormData):**
```
file: File
productId: uuid
```

#### 5. Удалить вложение
```http
DELETE /api/attachment/{id}
```

---

## Files Service API

### Files Controller

#### 1. Загрузить изображение
```http
POST /api/files/upload
Content-Type: multipart/form-data
```

**Request Body (FormData):**
```
file: File (только изображения: jpeg, png, gif, webp)
```

**Response:**
```json
{
  "fileName": "string",
  "message": "File uploaded successfully"
}
```

#### 2. Получить изображение по имени файла
```http
GET /api/files/file/{fileName}
```

**Response:** Binary image data

#### 3. Получить изображение (альтернативный endpoint)
```http
GET /api/files/{fileName}
```

**Response:** Binary image data (MIME: image/jpeg)

#### 4. Удалить изображение
```http
DELETE /api/files/{fileName}
```

**Response:**
```json
"Image deleted successfully"
```

#### 5. Получить все изображения
```http
GET /api/files
```

**Response:**
```json
[
  {
    "name": "string",
    "size": "long",
    "lastModified": "datetime"
  }
]
```

---

## Коды ответов

### Успешные ответы:
- **200 OK** - Запрос выполнен успешно
- **201 Created** - Ресурс создан успешно
- **204 No Content** - Запрос выполнен успешно, содержимое отсутствует

### Ошибки клиента:
- **400 Bad Request** - Неверный запрос
- **404 Not Found** - Ресурс не найден

### Ошибки сервера:
- **500 Internal Server Error** - Внутренняя ошибка сервера


## Примечания

1. Файлы изображений должны быть форматов: JPEG, PNG, GIF, WebP
2. Размер файлов ограничен настройками сервера
3. Для загрузки файлов используйте `Content-Type: multipart/form-data`
4. Для JSON данных используйте `Content-Type: application/json`