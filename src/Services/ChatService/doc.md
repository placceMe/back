# ChatService Implementation Documentation - React + TypeScript

## Architecture Overview

ChatService follows the **marketplace microservices architecture** with:

- **Database Schema Isolation**: PostgreSQL with `chat_service` schema
- **Inter-Service Communication**: HTTP clients to UsersService and ProductsService
- **Hybrid Communication Pattern**: REST API for CRUD + SignalR for real-time events
- **Redis Integration**: Caching and real-time notifications
- **NGINX Routing**: WebSocket upgrade support for SignalR

## Service Integration Points

### Database Schema Pattern

```sql
-- ChatService uses isolated schema
ConnectionString: "Host=postgres-db;Database=marketplace_db;Search Path=chat_service;"
```

### Inter-Service HTTP Clients

```csharp
// Follow marketplace pattern for service communication
builder.Services.AddHttpClient<IUsersServiceClient>((serviceProvider, client) => {
    client.BaseAddress = new Uri(configuration["UsersService:BaseUrl"]);
});

builder.Services.AddHttpClient<IProductsServiceClient>((serviceProvider, client) => {
    client.BaseAddress = new Uri(configuration["ProductsService:BaseUrl"]);
});
```

### Conversation Key Format

```
{productId}-{salerId}-{buyerId}
```

This ensures unique conversations per product-seller-buyer combination.

## Frontend Implementation Strategy

### 1. Service Layer Architecture

Create service layers following marketplace patterns:

```typescript
// services/ChatApiService.ts - REST API calls
// services/ChatSignalRService.ts - WebSocket management
// hooks/useChatService.ts - React hook combining both
// types/chat.ts - TypeScript interfaces
```

### 2. Component Structure

```
components/
├── chat/
│   ├── ChatList.tsx           # Conversations list
│   ├── ChatWindow.tsx         # Chat interface
│   ├── MessageBubble.tsx      # Individual message
│   └── UnreadBadge.tsx        # Unread indicator
├── product/
│   └── ContactSellerButton.tsx # Product page integration
└── seller/
    └── SellerChatDashboard.tsx # Seller chat management
```

### 3. State Management Pattern

Follow marketplace state patterns:

```typescript
// Use React Context for global chat state
// Local state for individual chat windows
// Redux/Zustand if already used in the project
```

## Key Implementation Requirements

### Authentication Integration

```typescript
// Must integrate with existing marketplace auth
// Use JWT tokens from UsersService
// Handle token refresh for SignalR connections
```

### CORS Configuration

```csharp
// ChatService appsettings.json
"AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
```

### NGINX Configuration

```nginx
# Add to existing nginx.conf
location /api/chat/ {
    proxy_pass http://chat-service:80/api/chat/;
}

location /chathub {
    proxy_pass http://chat-service:80/chathub;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;
}
```

## REST API Implementation Guide

### Core Endpoints

```http
# Follow marketplace RESTful patterns
GET    /api/chat/conversations/{productId}/{salerId}/{buyerId}
GET    /api/chat/conversations/user/{userId}
GET    /api/chat/conversations/{conversationKey}/messages
POST   /api/chat/messages
GET    /api/chat/conversations/{conversationKey}/unread-count/{userId}
GET    /api/chat/health
```

### Request/Response Patterns

```typescript
// Follow marketplace API response patterns
interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
```

## SignalR Implementation Guide

### Connection Management

```typescript
// Use marketplace auth tokens for SignalR
// Handle automatic reconnection
// Manage connection state across components
// Graceful degradation when WebSocket unavailable
```

### Event Handling

```typescript
// Core SignalR events to implement:
// - ReceiveMessage: New message received
// - MessagesMarkedAsRead: Read receipts
// - Error: Error handling
// - ConnectionStatus: Connection state changes
```

### Group Management

```typescript
// Join conversation groups: `conversationKey`
// Join user channels: `user_{userId}`
// Leave groups on component unmount
```

## Data Flow Patterns

### Sending Messages

```
User Input → Validation → REST API Call → SignalR Broadcast → UI Update
```

### Receiving Messages

```
SignalR Event → State Update → UI Re-render → Auto-mark as read (if viewing)
```

### Read Receipts

```
User Views Chat → SignalR invoke MarkAsRead → Database Update → Broadcast to all participants
```

## Error Handling Strategy

### Network Errors

```typescript
// Implement retry logic for API calls
// Queue messages when offline
// Show connection status to users
// Graceful fallback when SignalR fails
```

### Validation Errors

```typescript
// Client-side message validation
// Server-side error response handling
// User-friendly error messages
// Form validation feedback
```

## Performance Considerations

### Message Loading

```typescript
// Implement pagination for message history
// Virtual scrolling for large conversations
// Lazy loading of conversation list
// Image/file preview optimization
```

### Real-time Updates

```typescript
// Debounce typing indicators
// Throttle scroll-based read receipts
// Efficient state updates to prevent re-renders
// Memory management for long conversations
```

## Testing Strategy

### Unit Tests

```typescript
// Test chat service functions
// Test SignalR event handlers
// Test message validation
// Test state management logic
```

### Integration Tests

```typescript
// Test REST API endpoints
// Test SignalR connection flows
// Test error scenarios
// Test authentication integration
```

### E2E Tests

```typescript
// Test complete chat workflows
// Test real-time message delivery
// Test read receipt functionality
// Test offline/online scenarios
```

## Security Implementation

### Message Validation

```typescript
// Sanitize message content
// Validate conversation access
// Rate limiting for message sending
// XSS protection for message display
```

### Authentication

```typescript
// JWT token validation for REST API
// Token refresh for long-lived SignalR connections
// User role-based access control
// Conversation participant verification
```

## Deployment Integration

### Docker Compose

```yaml
# ChatService must depend on:
# - postgres-db (for database)
# - redis (for caching)
# - users-service (for user data)
# - products-service (for product data)
```

### Environment Variables

```env
# Required configuration
ASPNETCORE_ENVIRONMENT=Production
DATABASE_CONNECTION_STRING=...
REDIS_CONNECTION_STRING=...
USERS_SERVICE_URL=http://users-service:80
PRODUCTS_SERVICE_URL=http://products-service:80
JWT_SECRET_KEY=...
ALLOWED_ORIGINS=https://yourdomain.com
```

## Migration Strategy

### Database Setup

```csharp
// Use marketplace migration patterns
// Apply schema isolation
// Handle initial data seeding
// Plan for future schema changes
```

### Gradual Rollout

```typescript
// Feature flags for chat functionality
// Progressive enhancement approach
// Backward compatibility considerations
// Performance monitoring during rollout
```

## Monitoring and Logging

### Logging Strategy

```csharp
// Use Serilog with PostgreSQL sink (marketplace pattern)
// Log conversation events
// Log SignalR connection events
// Log performance metrics
// Log error scenarios
```

### Health Checks

```csharp
// Database connectivity
// Redis connectivity
// SignalR hub health
// Dependent service availability
```

## Specific Frontend Implementation Examples

### TypeScript Types

```typescript
// types/chat.ts
export interface ChatMessage {
  id: string;
  conversationKey: string;
  senderId: number;
  message: string;
  createdAt: string;
  isRead: boolean;
}

export interface Conversation {
  conversationKey: string;
  productId: number;
  salerId: number;
  buyerId: number;
  createdAt: string;
  lastMessageAt: string;
  messages: ChatMessage[];
}

export interface ConversationSummary {
  conversationKey: string;
  productId: number;
  salerId: number;
  buyerId: number;
  lastMessageAt: string;
  unreadCount: number;
  lastMessage?: ChatMessage;
  productTitle?: string;
  productImageUrl?: string;
  otherUserName?: string;
  otherUserAvatar?: string;
}
```

### React Hook Pattern

```typescript
// hooks/useChatService.ts
export const useChatService = ({
  userId,
  baseUrl = '/api/chat',
  onNewMessage,
  onMessagesRead,
  onError
}: UseChatServiceOptions) => {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  
  // Initialize SignalR connection
  const initializeConnection = useCallback(async () => {
    // Connection setup with marketplace auth
  }, [userId]);

  // REST API methods
  const sendMessage = useCallback(async (conversationKey: string, message: string) => {
    // API call implementation
  }, [baseUrl, userId]);

  // SignalR methods
  const joinConversation = useCallback(async (conversationKey: string) => {
    // SignalR group join
  }, [isConnected]);

  return {
    isConnected,
    sendMessage,
    joinConversation,
    // ... other methods
  };
};
```

### Component Implementation

```typescript
// components/ChatWindow.tsx
export const ChatWindow: React.FC<ChatWindowProps> = ({ 
  conversationKey, 
  userId, 
  onClose 
}) => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [newMessage, setNewMessage] = useState('');
  
  const chatService = useChatService({
    userId,
    onNewMessage: (message) => {
      if (message.conversationKey === conversationKey) {
        setMessages(prev => [...prev, message]);
        // Auto-mark as read
        chatService.markAsRead(conversationKey);
      }
    }
  });

  // Component implementation
};
```

## Integration with Existing Marketplace Services

### UsersService Integration

```typescript
// Get user info for chat participants
const getUserInfo = async (userId: number) => {
  const response = await fetch(`/api/users/${userId}`);
  return await response.json();
};
```

### ProductsService Integration

```typescript
// Get product info for chat context
const getProductInfo = async (productId: number) => {
  const response = await fetch(`/api/products/${productId}`);
  return await response.json();
};
```

### Authentication Flow

```typescript
// Use existing marketplace JWT tokens
const token = localStorage.getItem('accessToken');
const headers = {
  'Authorization': `Bearer ${token}`,
  'Content-Type': 'application/json'
};
```

This documentation provides the complete implementation strategy for integrating ChatService into the marketplace ecosystem while following established architectural patterns and conventions.