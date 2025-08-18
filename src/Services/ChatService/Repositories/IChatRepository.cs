using ChatService.Models;

namespace ChatService.Repositories;

public interface IChatRepository
{
    Task<Chat?> GetChatAsync(Guid sellerId, Guid buyerId, Guid productId);
    Task<Chat?> GetChatByIdAsync(Guid chatId);
    Task<List<Chat>> GetUserChatsAsync(Guid userId);
    Task<Chat> CreateChatAsync(Chat chat);
    Task<ChatMessage> AddMessageAsync(ChatMessage message);
    Task UpdateChatLastMessageTimeAsync(Guid chatId);
    Task MarkMessagesAsReadAsync(Guid chatId, Guid userId);
    Task<List<ChatMessage>> GetChatMessagesAsync(Guid chatId, int page, int pageSize);
}