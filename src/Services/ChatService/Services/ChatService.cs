using ChatService.Models;
using ChatService.Repositories;

namespace ChatService.Services;

public class ChatServiceImplementation : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly ILogger<ChatServiceImplementation> _logger;

    public ChatServiceImplementation(IChatRepository chatRepository, ILogger<ChatServiceImplementation> logger)
    {
        _chatRepository = chatRepository;
        _logger = logger;
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

        var createdChat = await _chatRepository.CreateChatAsync(chat);
        _logger.LogInformation("Created new chat {ChatId} between seller {SellerId} and buyer {BuyerId} for product {ProductId}",
            chat.Id, sellerId, buyerId, productId);

        return createdChat;
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

        _logger.LogInformation("Message {MessageId} sent in chat {ChatId} by user {SenderId}",
            message.Id, chatId, senderId);

        return createdMessage;
    }

    public async Task MarkMessagesAsReadAsync(Guid chatId, Guid userId)
    {
        await _chatRepository.MarkMessagesAsReadAsync(chatId, userId);
        _logger.LogInformation("Messages marked as read in chat {ChatId} by user {UserId}", chatId, userId);
    }

    public async Task<List<ChatMessage>> GetChatMessagesAsync(Guid chatId, int page = 1, int pageSize = 50)
    {
        return await _chatRepository.GetChatMessagesAsync(chatId, page, pageSize);
    }
}