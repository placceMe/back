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

# ChatService Frontend Integration Guide

## 📋 Огляд

Цей документ описує інтеграцію React + TypeScript фронтенду з ChatService для реалізації системи чатів у маркетплейсі.

## 🏗️ Архітектура Frontend

### Технологічний стек
- **React 18+** з TypeScript
- **@microsoft/signalr** для real-time комунікації
- **Custom Hooks** для бізнес-логіки
- **Context API** для глобального стану чатів

### Структура проекту
```
src/
  hooks/
    useSignalR.ts          # SignalR підключення
    useChat.ts             # Управління чатами
    useChatMessages.ts     # Повідомлення чату
    useNotifications.ts    # Нотифікації
  types/
    chat.types.ts          # TypeScript типи
  components/
    ChatList.tsx           # Список чатів
    ChatWindow.tsx         # Вікно чату
    MessageInput.tsx       # Поле вводу
    NotificationBadge.tsx  # Бейдж нотифікацій
  contexts/
    ChatContext.tsx        # Глобальний стан чатів
```

## 🔌 SignalR Інтеграція

### Встановлення залежностей
```sh
npm install @microsoft/signalr
npm install @types/node  # для UUID типів
```

### Custom Hook: useSignalR

```typescript
// hooks/useSignalR.ts
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useState, useEffect, useCallback } from 'react';

export interface UseSignalROptions {
  url: string;
  automaticReconnect?: boolean;
  logging?: LogLevel;
}

export interface UseSignalRReturn {
  connection: HubConnection | null;
  isConnected: boolean;
  isConnecting: boolean;
  error: string | null;
  startConnection: () => Promise<void>;
  stopConnection: () => Promise<void>;
}

export const useSignalR = ({
  url,
  automaticReconnect = true,
  logging = LogLevel.Information
}: UseSignalROptions): UseSignalRReturn => {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [isConnecting, setIsConnecting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const startConnection = useCallback(async () => {
    if (connection?.state === 'Connected') return;
    
    setIsConnecting(true);
    setError(null);

    try {
      const hubConnection = new HubConnectionBuilder()
        .withUrl(url)
        .configureLogging(logging);

      if (automaticReconnect) {
        hubConnection.withAutomaticReconnect();
      }

      const builtConnection = hubConnection.build();

      // Обробка подій з'єднання
      builtConnection.onclose((error) => {
        setIsConnected(false);
        if (error) {
          setError(error.message);
        }
      });

      builtConnection.onreconnecting((error) => {
        setIsConnected(false);
        setError(error ? error.message : 'Reconnecting...');
      });

      builtConnection.onreconnected(() => {
        setIsConnected(true);
        setError(null);
      });

      await builtConnection.start();
      setConnection(builtConnection);
      setIsConnected(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Connection failed');
    } finally {
      setIsConnecting(false);
    }
  }, [url, automaticReconnect, logging, connection]);

  const stopConnection = useCallback(async () => {
    if (connection) {
      await connection.stop();
      setConnection(null);
      setIsConnected(false);
    }
  }, [connection]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (connection) {
        connection.stop();
      }
    };
  }, [connection]);

  return {
    connection,
    isConnected,
    isConnecting,
    error,
    startConnection,
    stopConnection
  };
};
```

## 📨 Типи даних

```typescript
// types/chat.types.ts
export interface Chat {
  id: string;
  productId: string;
  sellerId: string;
  buyerId: string;
  createdAt: string;
}

export interface ChatMessage {
  id: string;
  chatId: string;
  senderUserId: string;
  body: string;
  createdAt: string;
}

export interface MessageNotification {
  chatId: string;
  productId: string;
  senderUserId: string;
  senderName: string;
  messagePreview: string;
  createdAt: string;
  productTitle: string;
  productImageUrl?: string;
}

export interface CreateChatRequest {
  productId: string;
  sellerId: string;
  buyerId: string;
}

export interface CreateMessageRequest {
  senderUserId: string;
  body: string;
}

export interface ChatUser {
  id: string;
  name: string;
  avatarUrl?: string;
}

export interface ChatWithDetails extends Chat {
  productTitle?: string;
  productImageUrl?: string;
  lastMessage?: ChatMessage;
  unreadCount: number;
  otherParticipant?: ChatUser;
}
```

## 🎣 Custom Hooks для бізнес-логіки

### Hook для управління чатами

```typescript
// hooks/useChat.ts
import { useState, useCallback } from 'react';
import { Chat, CreateChatRequest } from '../types/chat.types';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5015';

export interface UseChatReturn {
  chats: Chat[];
  loading: boolean;
  error: string | null;
  loadUserChats: (userId: string) => Promise<void>;
  createChat: (request: CreateChatRequest) => Promise<Chat | null>;
  refreshChats: () => Promise<void>;
}

export const useChat = (currentUserId: string | null): UseChatReturn => {
  const [chats, setChats] = useState<Chat[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadUserChats = useCallback(async (userId: string) => {
    if (!userId) return;

    setLoading(true);
    setError(null);

    try {
      // Завантажуємо чати як seller та buyer
      const [sellerResponse, buyerResponse] = await Promise.all([
        fetch(`${API_BASE_URL}/api/chats?sellerId=${userId}`),
        fetch(`${API_BASE_URL}/api/chats?buyerId=${userId}`)
      ]);

      const sellerChats = sellerResponse.ok ? await sellerResponse.json() : [];
      const buyerChats = buyerResponse.ok ? await buyerResponse.json() : [];

      // Об'єднуємо та дедуплікуємо
      const allChats = [...sellerChats, ...buyerChats];
      const uniqueChats = allChats.filter((chat, index, self) => 
        index === self.findIndex(c => c.id === chat.id)
      );

      setChats(uniqueChats);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load chats');
    } finally {
      setLoading(false);
    }
  }, []);

  const createChat = useCallback(async (request: CreateChatRequest): Promise<Chat | null> => {
    setLoading(true);
    setError(null);

    try {
      const response = await fetch(`${API_BASE_URL}/api/chats`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
      });

      if (!response.ok) {
        throw new Error(`Failed to create chat: ${response.status}`);
      }

      const newChat = await response.json();
      
      // Додаємо до існуючих чатів
      setChats(prev => [newChat, ...prev]);
      
      return newChat;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create chat');
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const refreshChats = useCallback(async () => {
    if (currentUserId) {
      await loadUserChats(currentUserId);
    }
  }, [currentUserId, loadUserChats]);

  return {
    chats,
    loading,
    error,
    loadUserChats,
    createChat,
    refreshChats
  };
};
```

### Hook для повідомлень чату

```typescript
// hooks/useChatMessages.ts
import { useState, useCallback } from 'react';
import { ChatMessage, CreateMessageRequest } from '../types/chat.types';

export interface UseChatMessagesReturn {
  messages: ChatMessage[];
  loading: boolean;
  error: string | null;
  loadMessages: (chatId: string) => Promise<void>;
  sendMessage: (chatId: string, request: CreateMessageRequest) => Promise<boolean>;
  addMessage: (message: ChatMessage) => void;
  clearMessages: () => void;
}

export const useChatMessages = (): UseChatMessagesReturn => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadMessages = useCallback(async (chatId: string) => {
    setLoading(true);
    setError(null);

    try {
      const response = await fetch(`${API_BASE_URL}/api/chats/${chatId}/messages`);
      
      if (!response.ok) {
        throw new Error(`Failed to load messages: ${response.status}`);
      }

      const chatMessages = await response.json();
      setMessages(chatMessages);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load messages');
    } finally {
      setLoading(false);
    }
  }, []);

  const sendMessage = useCallback(async (
    chatId: string, 
    request: CreateMessageRequest
  ): Promise<boolean> => {
    setError(null);

    try {
      const response = await fetch(`${API_BASE_URL}/api/chats/${chatId}/messages`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
      });

      if (!response.ok) {
        throw new Error(`Failed to send message: ${response.status}`);
      }

      // Повідомлення буде додано через SignalR
      return true;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to send message');
      return false;
    }
  }, []);

  const addMessage = useCallback((message: ChatMessage) => {
    setMessages(prev => [...prev, message]);
  }, []);

  const clearMessages = useCallback(() => {
    setMessages([]);
  }, []);

  return {
    messages,
    loading,
    error,
    loadMessages,
    sendMessage,
    addMessage,
    clearMessages
  };
};
```

### Hook для нотифікацій

```typescript
// hooks/useNotifications.ts
import { useState, useCallback } from 'react';
import { MessageNotification } from '../types/chat.types';

export interface UseNotificationsReturn {
  notifications: MessageNotification[];
  unreadCount: number;
  addNotification: (notification: MessageNotification) => void;
  markAsRead: (chatId: string) => void;
  markAllAsRead: () => void;
  clearNotifications: () => void;
}

export const useNotifications = (): UseNotificationsReturn => {
  const [notifications, setNotifications] = useState<MessageNotification[]>([]);

  const addNotification = useCallback((notification: MessageNotification) => {
    setNotifications(prev => [notification, ...prev].slice(0, 50)); // Keep last 50
  }, []);

  const markAsRead = useCallback((chatId: string) => {
    setNotifications(prev => prev.filter(n => n.chatId !== chatId));
  }, []);

  const markAllAsRead = useCallback(() => {
    setNotifications([]);
  }, []);

  const clearNotifications = useCallback(() => {
    setNotifications([]);
  }, []);

  const unreadCount = notifications.length;

  return {
    notifications,
    unreadCount,
    addNotification,
    markAsRead,
    markAllAsRead,
    clearNotifications
  };
};
```

## 🌐 Контекст для глобального стану

```typescript
// contexts/ChatContext.tsx
import React, { createContext, useContext, useEffect, ReactNode } from 'react';
import { useSignalR } from '../hooks/useSignalR';
import { useChat } from '../hooks/useChat';
import { useNotifications } from '../hooks/useNotifications';
import { ChatMessage, MessageNotification } from '../types/chat.types';

interface ChatContextType {
  // SignalR
  connection: any;
  isConnected: boolean;
  isConnecting: boolean;
  
  // Chats
  chats: any[];
  loadUserChats: (userId: string) => Promise<void>;
  createChat: any;
  
  // Notifications
  notifications: MessageNotification[];
  unreadCount: number;
  markAsRead: (chatId: string) => void;
  
  // Current user
  currentUserId: string | null;
  setCurrentUserId: (userId: string | null) => void;
  
  // Chat operations
  joinChat: (chatId: string) => Promise<void>;
  leaveChat: (chatId: string) => Promise<void>;
  subscribeToNotifications: (userId: string) => Promise<void>;
}

const ChatContext = createContext<ChatContextType | undefined>(undefined);

interface ChatProviderProps {
  children: ReactNode;
  signalRUrl?: string;
}

export const ChatProvider: React.FC<ChatProviderProps> = ({ 
  children, 
  signalRUrl = '/hubs/chat' 
}) => {
  const [currentUserId, setCurrentUserId] = React.useState<string | null>(null);
  
  const { connection, isConnected, isConnecting, startConnection } = useSignalR({
    url: signalRUrl,
    automaticReconnect: true
  });

  const { chats, loadUserChats, createChat } = useChat(currentUserId);
  const { notifications, unreadCount, addNotification, markAsRead } = useNotifications();

  // Ініціалізація SignalR обробників
  useEffect(() => {
    if (!connection) return;

    // Обробник нотифікацій
    connection.on('MessageNotification', (notification: MessageNotification) => {
      addNotification(notification);
      
      // Показати браузерну нотифікацію (опціонально)
      if (Notification.permission === 'granted') {
        new Notification(`Нове повідомлення від ${notification.senderName}`, {
          body: notification.messagePreview,
          icon: notification.productImageUrl
        });
      }
    });

    // Очищення обробників при unmount
    return () => {
      connection.off('MessageNotification');
    };
  }, [connection, addNotification]);

  // Методи для роботи з чатами
  const joinChat = async (chatId: string) => {
    if (connection && isConnected) {
      await connection.invoke('JoinChat', chatId);
    }
  };

  const leaveChat = async (chatId: string) => {
    if (connection && isConnected) {
      await connection.invoke('LeaveChat', chatId);
    }
  };

  const subscribeToNotifications = async (userId: string) => {
    if (connection && isConnected) {
      await connection.invoke('SubscribeToUserNotifications', userId);
    }
  };

  // Автоматичне підключення при зміні користувача
  useEffect(() => {
    if (currentUserId && !isConnected && !isConnecting) {
      startConnection();
    }
  }, [currentUserId, isConnected, isConnecting, startConnection]);

  // Автоматична підписка на нотифікації
  useEffect(() => {
    if (currentUserId && isConnected) {
      subscribeToNotifications(currentUserId);
      loadUserChats(currentUserId);
    }
  }, [currentUserId, isConnected]);

  const value: ChatContextType = {
    connection,
    isConnected,
    isConnecting,
    chats,
    loadUserChats,
    createChat,
    notifications,
    unreadCount,
    markAsRead,
    currentUserId,
    setCurrentUserId,
    joinChat,
    leaveChat,
    subscribeToNotifications
  };

  return (
    <ChatContext.Provider value={value}>
      {children}
    </ChatContext.Provider>
  );
};

export const useChatContext = () => {
  const context = useContext(ChatContext);
  if (undefined === context) {
    throw new Error('useChatContext must be used within a ChatProvider');
  }
  return context;
};
```

## 🧩 Компоненти

### Компонент списку чатів

```typescript
// components/ChatList.tsx
import React, { useState, useEffect } from 'react';
import { useChatContext } from '../contexts/ChatContext';
import { ChatWithDetails } from '../types/chat.types';

interface ChatListProps {
  onChatSelect: (chatId: string) => void;
  selectedChatId?: string;
  className?: string;
}

export const ChatList: React.FC<ChatListProps> = ({ 
  onChatSelect, 
  selectedChatId, 
  className = '' 
}) => {
  const { chats, currentUserId, notifications, unreadCount } = useChatContext();
  const [chatsWithDetails, setChatsWithDetails] = useState<ChatWithDetails[]>([]);

  // Додаємо деталі до чатів (останнє повідомлення, непрочитані тощо)
  useEffect(() => {
    const enrichChats = async () => {
      const enriched = await Promise.all(
        chats.map(async (chat) => {
          // Підрахунок непрочитаних для цього чату
          const unreadForChat = notifications.filter(n => n.chatId === chat.id).length;
          
          // Визначення іншого учасника
          const isUserSeller = chat.sellerId === currentUserId;
          const otherParticipantId = isUserSeller ? chat.buyerId : chat.sellerId;
          
          // Тут можна додати запит для отримання інформації про користувача
          // const otherParticipant = await getUserInfo(otherParticipantId);
          
          return {
            ...chat,
            unreadCount: unreadForChat,
            otherParticipant: {
              id: otherParticipantId,
              name: `User ${otherParticipantId.substring(0, 8)}...`,
              avatarUrl: undefined
            }
          } as ChatWithDetails;
        })
      );
      
      // Сортуємо за часом створення (новіші спочатку)
      enriched.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
      
      setChatsWithDetails(enriched);
    };

    enrichChats();
  }, [chats, notifications, currentUserId]);

  if (chats.length === 0) {
    return (
      <div className={`chat-list empty ${className}`}>
        <div className="empty-state">
          <div className="empty-icon">💬</div>
          <h3>Поки немає чатів</h3>
          <p>Розпочніть спілкування з продавцями товарів</p>
        </div>
      </div>
    );
  }

  return (
    <div className={`chat-list ${className}`}>
      <div className="chat-list-header">
        <h2>
          Чати 
          {unreadCount > 0 && (
            <span className="unread-badge">{unreadCount}</span>
          )}
        </h2>
      </div>
      
      <div className="chat-list-items">
        {chatsWithDetails.map((chat) => (
          <ChatListItem
            key={chat.id}
            chat={chat}
            isSelected={chat.id === selectedChatId}
            onClick={() => onChatSelect(chat.id)}
          />
        ))}
      </div>
    </div>
  );
};

interface ChatListItemProps {
  chat: ChatWithDetails;
  isSelected: boolean;
  onClick: () => void;
}

const ChatListItem: React.FC<ChatListItemProps> = ({ chat, isSelected, onClick }) => {
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffTime = Math.abs(now.getTime() - date.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays === 1) {
      return 'Сьогодні';
    } else if (diffDays === 2) {
      return 'Вчора';
    } else if (diffDays <= 7) {
      return `${diffDays} днів тому`;
    } else {
      return date.toLocaleDateString('uk-UA');
    }
  };

  return (
    <div 
      className={`chat-list-item ${isSelected ? 'selected' : ''} ${chat.unreadCount > 0 ? 'unread' : ''}`}
      onClick={onClick}
    >
      <div className="chat-avatar">
        {chat.otherParticipant?.avatarUrl ? (
          <img 
            src={chat.otherParticipant.avatarUrl} 
            alt={chat.otherParticipant.name}
            className="avatar-image"
          />
        ) : (
          <div className="avatar-placeholder">
            {chat.otherParticipant?.name?.charAt(0) || '?'}
          </div>
        )}
      </div>

      <div className="chat-content">
        <div className="chat-header">
          <h4 className="chat-title">
            {chat.otherParticipant?.name || 'Невідомий користувач'}
          </h4>
          <span className="chat-date">
            {formatDate(chat.createdAt)}
          </span>
        </div>

        <div className="chat-preview">
          <span className="product-info">
            📦 Товар: {chat.productId.substring(0, 8)}...
          </span>
          {chat.lastMessage && (
            <p className="last-message">
              {chat.lastMessage.body}
            </p>
          )}
        </div>
      </div>

      {chat.unreadCount > 0 && (
        <div className="unread-indicator">
          <span className="unread-count">{chat.unreadCount}</span>
        </div>
      )}
    </div>
  );
};
```

### Основний чат компонент

```typescript
// components/ChatWindow.tsx
import React, { useEffect, useState } from 'react';
import { useChatMessages } from '../hooks/useChatMessages';
import { useChatContext } from '../contexts/ChatContext';
import { MessageInput } from './MessageInput';

interface ChatWindowProps {
  chatId: string;
  onClose?: () => void;
}

export const ChatWindow: React.FC<ChatWindowProps> = ({ chatId, onClose }) => {
  const { connection, isConnected, currentUserId, joinChat, leaveChat, markAsRead } = useChatContext();
  const { messages, loadMessages, addMessage, clearMessages } = useChatMessages();
  const [isJoined, setIsJoined] = useState(false);

  // Завантаження повідомлень при відкритті чату
  useEffect(() => {
    if (chatId) {
      loadMessages(chatId);
      markAsRead(chatId); // Позначити як прочитані
    }
    
    return () => clearMessages();
  }, [chatId]);

  // Приєднання до чату через SignalR
  useEffect(() => {
    if (chatId && isConnected && !isJoined) {
      joinChat(chatId);
      setIsJoined(true);
    }

    return () => {
      if (chatId && isJoined) {
        leaveChat(chatId);
        setIsJoined(false);
      }
    };
  }, [chatId, isConnected]);

  // Обробка живих повідомлень
  useEffect(() => {
    if (!connection) return;

    const handleMessage = (message: any) => {
      if (message.chatId === chatId) {
        addMessage(message);
      }
    };

    connection.on('MessageCreated', handleMessage);
    
    return () => {
      connection.off('MessageCreated', handleMessage);
    };
  }, [connection, chatId, addMessage]);

  return (
    <div className="chat-window">
      <div className="chat-header">
        <h3>Chat {chatId.substring(0, 8)}...</h3>
        {onClose && (
          <button onClick={onClose} className="close-btn">✕</button>
        )}
      </div>

      <div className="messages-container">
        {messages.map((message) => (
          <div 
            key={message.id} 
            className={`message ${message.senderUserId === currentUserId ? 'own' : 'other'}`}
          >
            <div className="message-content">{message.body}</div>
            <div className="message-time">
              {new Date(message.createdAt).toLocaleTimeString()}
            </div>
          </div>
        ))}
      </div>

      <MessageInput 
        chatId={chatId} 
        disabled={!isConnected || !currentUserId} 
      />
    </div>
  );
};
```

### Компонент для вводу повідомлень

```typescript
// components/MessageInput.tsx
import React, { useState } from 'react';
import { useChatMessages } from '../hooks/useChatMessages';
import { useChatContext } from '../contexts/ChatContext';

interface MessageInputProps {
  chatId: string;
  disabled?: boolean;
}

export const MessageInput: React.FC<MessageInputProps> = ({ chatId, disabled }) => {
  const [text, setText] = useState('');
  const [sending, setSending] = useState(false);
  const { sendMessage } = useChatMessages();
  const { currentUserId } = useChatContext();

  const handleSend = async () => {
    if (!text.trim() || !currentUserId || sending) return;

    setSending(true);
    const success = await sendMessage(chatId, {
      senderUserId: currentUserId,
      body: text.trim()
    });

    if (success) {
      setText('');
    }
    setSending(false);
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <div className="message-input">
      <textarea
        value={text}
        onChange={(e) => setText(e.target.value)}
        onKeyPress={handleKeyPress}
        placeholder="Введіть повідомлення..."
        disabled={disabled || sending}
        rows={3}
      />
      <button 
        onClick={handleSend} 
        disabled={!text.trim() || disabled || sending}
        className="send-btn"
      >
        {sending ? '⏳' : '📤'}
      </button>
    </div>
  );
};
```

### Компонент бейджу нотифікацій

```typescript
// components/NotificationBadge.tsx
import React from 'react';
import { useChatContext } from '../contexts/ChatContext';

interface NotificationBadgeProps {
  className?: string;
  onClick?: () => void;
}

export const NotificationBadge: React.FC<NotificationBadgeProps> = ({ 
  className = '', 
  onClick 
}) => {
  const { unreadCount, notifications } = useChatContext();

  if (unreadCount === 0) {
    return null;
  }

  return (
    <div 
      className={`notification-badge ${className}`}
      onClick={onClick}
      title={`${unreadCount} нових повідомлень`}
    >
      <span className="badge-icon">💬</span>
      <span className="badge-count">{unreadCount > 99 ? '99+' : unreadCount}</span>
      
      {/* Dropdown з останніми нотифікаціями */}
      <div className="notification-dropdown">
        <div className="notification-header">
          <h4>Нові повідомлення</h4>
        </div>
        <div className="notification-list">
          {notifications.slice(0, 5).map((notification) => (
            <div key={`${notification.chatId}-${notification.createdAt}`} className="notification-item">
              <div className="notification-avatar">
                {notification.productImageUrl ? (
                  <img src={notification.productImageUrl} alt={notification.productTitle} />
                ) : (
                  <div className="placeholder">📦</div>
                )}
              </div>
              <div className="notification-content">
                <h5>{notification.senderName}</h5>
                <p>{notification.messagePreview}</p>
                <small>{notification.productTitle}</small>
              </div>
            </div>
          ))}
        </div>
        {unreadCount > 5 && (
          <div className="notification-footer">
            <small>+ ще {unreadCount - 5} повідомлень</small>
          </div>
        )}
      </div>
    </div>
  );
};
```

## 🚀 Використання у додатку

```typescript
// App.tsx
import React, { useState } from 'react';
import { ChatProvider } from './contexts/ChatContext';
import { ChatList } from './components/ChatList';
import { ChatWindow } from './components/ChatWindow';
import { NotificationBadge } from './components/NotificationBadge';

function App() {
  const [currentUserId, setCurrentUserId] = useState<string | null>(null);
  const [activeChatId, setActiveChatId] = useState<string | null>(null);
  const [showChatList, setShowChatList] = useState(false);

  return (
    <ChatProvider signalRUrl="http://localhost:5015/hubs/chat">
      <div className="app">
        {/* Основний контент сайту */}
        <header className="app-header">
          <h1>Marketplace</h1>
          
          {/* Бейдж нотифікацій */}
          <NotificationBadge 
            onClick={() => setShowChatList(true)}
            className="header-notifications"
          />
        </header>

        {/* Модальне вікно зі списком чатів */}
        {showChatList && (
          <div className="chat-modal">
            <div className="chat-modal-content">
              <ChatList 
                onChatSelect={(chatId) => {
                  setActiveChatId(chatId);
                  setShowChatList(false);
                }}
                selectedChatId={activeChatId}
              />
              <button 
                className="close-modal"
                onClick={() => setShowChatList(false)}
              >
                ✕
              </button>
            </div>
          </div>
        )}

        {/* Floating чат вікно */}
        {activeChatId && (
          <ChatWindow 
            chatId={activeChatId} 
            onClose={() => setActiveChatId(null)} 
          />
        )}
      </div>
    </ChatProvider>
  );
}

export default App;
```

## 🔔 Налаштування браузерних нотифікацій

```typescript
// utils/notifications.ts
export const requestNotificationPermission = async (): Promise<boolean> => {
  if (!('Notification' in window)) {
    console.log('This browser does not support notifications');
    return false;
  }

  if (Notification.permission === 'granted') {
    return true;
  }

  if (Notification.permission !== 'denied') {
    const permission = await Notification.requestPermission();
    return permission === 'granted';
  }

  return false;
};
```

## 📱 Адаптивність та стилізація

```css
/* styles/chat.css */
.chat-list {
  width: 300px;
  max-height: 500px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.2);
  overflow: hidden;
}

.chat-list-header {
  padding: 16px;
  background: #f8f9fa;
  border-bottom: 1px solid #e9ecef;
}

.chat-list-header h2 {
  margin: 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.unread-badge {
  background: #dc3545;
  color: white;
  border-radius: 50%;
  padding: 2px 6px;
  font-size: 12px;
  font-weight: bold;
}

.chat-list-items {
  max-height: 400px;
  overflow-y: auto;
}

.chat-list-item {
  display: flex;
  align-items: center;
  padding: 12px 16px;
  border-bottom: 1px solid #f1f3f5;
  cursor: pointer;
  transition: background-color 0.2s;
}

.chat-list-item:hover {
  background-color: #f8f9fa;
}

.chat-list-item.selected {
  background-color: #e3f2fd;
}

.chat-list-item.unread {
  background-color: #fff3e0;
  font-weight: 600;
}

.chat-avatar {
  width: 48px;
  height: 48px;
  margin-right: 12px;
  flex-shrink: 0;
}

.avatar-image {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  background: #6c757d;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 18px;
}

.chat-content {
  flex: 1;
  min-width: 0;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.chat-title {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: #212529;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.chat-date {
  font-size: 12px;
  color: #6c757d;
  flex-shrink: 0;
}

.chat-preview {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.product-info {
  font-size: 11px;
  color: #6c757d;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.last-message {
  margin: 0;
  font-size: 13px;
  color: #495057;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.unread-indicator {
  display: flex;
  align-items: center;
  margin-left: 8px;
}

.unread-count {
  background: #dc3545;
  color: white;
  border-radius: 50%;
  padding: 2px 6px;
  font-size: 11px;
  font-weight: bold;
  min-width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.chat-window {
  position: fixed;
  bottom: 20px;
  right: 20px;
  width: 350px;
  height: 500px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.2);
  display: flex;
  flex-direction: column;
  z-index: 1000;
}

.chat-header {
  padding: 16px;
  background: #007bff;
  color: white;
  border-radius: 12px 12px 0 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.chat-header h3 {
  margin: 0;
  font-size: 16px;
}

.close-btn {
  background: none;
  border: none;
  color: white;
  font-size: 20px;
  cursor: pointer;
  padding: 0;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.messages-container {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
}

.message {
  margin-bottom: 12px;
  padding: 8px 12px;
  border-radius: 18px;
  max-width: 80%;
  word-wrap: break-word;
}

.message.own {
  background: #007bff;
  color: white;
  margin-left: auto;
  text-align: right;
}

.message.other {
  background: #f1f3f5;
  color: black;
}

.message-time {
  font-size: 11px;
  opacity: 0.7;
  margin-top: 4px;
}

.message-input {
  display: flex;
  padding: 16px;
  gap: 8px;
  border-top: 1px solid #e9ecef;
}

.message-input textarea {
  flex: 1;
  border: 1px solid #ddd;
  border-radius: 20px;
  padding: 8px 16px;
  resize: none;
  font-family: inherit;
  outline: none;
}

.message-input textarea:focus {
  border-color: #007bff;
}

.send-btn {
  background: #007bff;
  color: white;
  border: none;
  border-radius: 50%;
  width: 40px;
  height: 40px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.2s;
}

.send-btn:hover {
  background: #0056b3;
}

.send-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.notification-badge {
  position: relative;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 8px;
  border-radius: 8px;
  transition: background-color 0.2s;
}

.notification-badge:hover {
  background-color: rgba(0,0,0,0.1);
}

.badge-icon {
  font-size: 20px;
}

.badge-count {
  background: #dc3545;
  color: white;
  border-radius: 50%;
  padding: 2px 6px;
  font-size: 12px;
  font-weight: bold;
  min-width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Мобільна адаптивність */
@media (max-width: 768px) {
  .chat-window {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    width: 100%;
    height: 100%;
    border-radius: 0;
  }

  .chat-list {
    width: 100%;
    max-height: 100%;
    border-radius: 0;
  }

  .chat-modal {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0,0,0,0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
  }

  .chat-modal-content {
    position: relative;
    width: 90%;
    height: 80%;
    max-width: 400px;
  }
}

.empty-state {
  text-align: center;
  padding: 40px 20px;
  color: #6c757d;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.empty-state h3 {
  margin: 0 0 8px 0;
  color: #495057;
}

.empty-state p {
  margin: 0;
  font-size: 14px;
}
```

## 🧪 Приклад інтеграції з існуючим додатком

```typescript
// pages/ProductPage.tsx
import React, { useState } from 'react';
import { useChatContext } from '../contexts/ChatContext';

interface ProductPageProps {
  productId: string;
  sellerId: string;
}

export const ProductPage: React.FC<ProductPageProps> = ({ productId, sellerId }) => {
  const { createChat, currentUserId } = useChatContext();
  const [chatId, setChatId] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleStartChat = async () => {
    if (!currentUserId) {
      // Redirect to login
      return;
    }

    setLoading(true);
    const chat = await createChat({
      productId,
      sellerId,
      buyerId: currentUserId
    });

    if (chat) {
      setChatId(chat.id);
    }
    setLoading(false);
  };

  return (
    <div className="product-page">
      {/* Контент товару */}
      
      <button 
        onClick={handleStartChat} 
        className="chat-btn"
        disabled={loading || !currentUserId}
      >
        {loading ? '⏳' : '💬'} Написати продавцю
      </button>

      {chatId && (
        <ChatWindow 
          chatId={chatId} 
          onClose={() => setChatId(null)} 
        />
      )}
    </div>
  );
};
```

## 🎯 Ключові особливості

### ✅ Переваги цього підходу:
- **Модульність** - кожна функціональність в окремому хуці
- **Типобезпека** - повна TypeScript підтримка
- **Реактивність** - автоматичні оновлення через SignalR
- **Масштабованість** - легко додавати нові функції
- **Тестованість** - хуки легко тестувати окремо
- **Користувацький досвід** - інтуїтивний інтерфейс зі списком чатів

### 🔧 Налаштування для production:
- Змініть `API_BASE_URL` в environment змінних
- Додайте error boundaries для обробки помилок
- Реалізуйте retry логіку для HTTP запитів
- Додайте метрики та логування
- Налаштуйте CORS для production доменів
- Оптимізуйте завантаження списку чатів (віртуалізація для великої кількості)

Ця архітектура забезпечує повну інтеграцію з ChatService, включаючи зручний список чатів для навігації між розмовами! 🚀
