using ChatService.Models;

namespace ChatService.Services;

public interface IChatService
{
    Task<Chat> CreateOrGetChatAsync(Guid sellerId, Guid buyerId, Guid productId);
    Task<Chat?> GetChatByIdAsync(Guid chatId);
    Task<List<Chat>> GetUserChatsAsync(Guid userId);
    Task<ChatMessage> SendMessageAsync(Guid chatId, Guid senderId, string content);
    Task MarkMessagesAsReadAsync(Guid chatId, Guid userId);
    Task<List<ChatMessage>> GetChatMessagesAsync(Guid chatId, int page = 1, int pageSize = 50);
}