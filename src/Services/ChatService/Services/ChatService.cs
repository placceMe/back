using ChatService.Models;
using ChatService.Repositories;

namespace ChatService.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;

    public ChatService(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    public async Task<Chat> CreateOrGetChatAsync(Guid sellerId, Guid buyerId, Guid productId)
    {
        var existingChat = await _chatRepository.GetChatAsync(sellerId, buyerId, productId);
        if (existingChat != null)
            return existingChat;

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            BuyerId = buyerId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow
        };

        return await _chatRepository.CreateChatAsync(chat);
    }

    public async Task<Chat?> GetChatByIdAsync(Guid chatId)
    {
        return await _chatRepository.GetChatByIdAsync(chatId);
    }

    public async Task<List<Chat>> GetUserChatsAsync(Guid userId)
    {
        return await _chatRepository.GetUserChatsAsync(userId);
    }

    public async Task<ChatMessage> SendMessageAsync(Guid chatId, Guid senderId, string content)
    {
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            SenderId = senderId,
            Content = content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        var createdMessage = await _chatRepository.AddMessageAsync(message);
        await _chatRepository.UpdateChatLastMessageTimeAsync(chatId);

        return createdMessage;
    }

    public async Task MarkMessagesAsReadAsync(Guid chatId, Guid userId)
    {
        await _chatRepository.MarkMessagesAsReadAsync(chatId, userId);
    }

    public async Task<List<ChatMessage>> GetChatMessagesAsync(Guid chatId, int page = 1, int pageSize = 50)
    {
        return await _chatRepository.GetChatMessagesAsync(chatId, page, pageSize);
    }
}