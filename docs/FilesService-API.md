# Files Service API Documentation

## Base URL
```
http://localhost:5001/api
```

## Files Controller

### 1. Upload Image
```http
POST /api/files/upload
Content-Type: multipart/form-data
```

**Request Body (FormData):**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| file | File | Yes | Image file (JPEG, PNG, GIF, WebP only) |

**Response:**
```json
{
  "fileName": "string",
  "message": "File uploaded successfully"
}
```

**Status Codes:**
- **200 OK**: File uploaded successfully
- **400 Bad Request**: Invalid file format or request
- **500 Internal Server Error**: Upload failed

### 2. Get Image by File Name
```http
GET /api/files/file/{fileName}
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| fileName | string | Yes | Name of the file to retrieve |

**Response:** Binary image data

**Status Codes:**
- **200 OK**: File retrieved successfully
- **404 Not Found**: File not found

### 3. Get Image (Alternative Endpoint)
```http
GET /api/files/{fileName}
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| fileName | string | Yes | Name of the file to retrieve |

**Response:** Binary image data (MIME: image/jpeg)

**Status Codes:**
- **200 OK**: File retrieved successfully
- **404 Not Found**: File not found

### 4. Delete Image
```http
DELETE /api/files/{fileName}
```

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| fileName | string | Yes | Name of the file to delete |

**Response:**
```json
"Image deleted successfully"
```

**Status Codes:**
- **200 OK**: File deleted successfully
- **404 Not Found**: File not found
- **500 Internal Server Error**: Deletion failed

### 5. Get All Images
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

**Status Codes:**
- **200 OK**: Files list retrieved successfully
- **500 Internal Server Error**: Failed to retrieve files list

---

## Supported File Formats

The Files Service accepts the following image formats:
- **JPEG** (.jpg, .jpeg)
- **PNG** (.png)
- **GIF** (.gif)
- **WebP** (.webp)

---

## File Storage

- Files are stored in **MinIO** object storage
- File names are automatically generated to prevent conflicts
- Original file extensions are preserved
- Files are organized in buckets for efficient retrieval

---

## Status Codes

### Success Responses:
- **200 OK** - Request completed successfully
- **201 Created** - File uploaded successfully

### Client Errors:
- **400 Bad Request** - Invalid file format or request
- **404 Not Found** - File not found

### Server Errors:
- **500 Internal Server Error** - Internal server error

---

## Usage Examples

### Upload an Image
```bash
curl -X POST "http://localhost:5001/api/files/upload" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/path/to/image.jpg"
```

### Get an Image
```bash
curl -X GET "http://localhost:5001/api/files/image.jpg" \
  --output downloaded-image.jpg
```

### Delete an Image
```bash
curl -X DELETE "http://localhost:5001/api/files/image.jpg"
```

---

## Notes

1. **File Size Limits**: Check server configuration for maximum file size
2. **Content-Type**: Always use `multipart/form-data` for file uploads
3. **File Names**: Use the returned fileName from upload response for subsequent operations
4. **CORS**: Service is configured to allow cross-origin requests for development
5. **Error Handling**: All endpoints return appropriate HTTP status codes and error messages