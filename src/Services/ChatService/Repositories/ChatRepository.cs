using Microsoft.EntityFrameworkCore;
using ChatService.Data;
using ChatService.Models;

namespace ChatService.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly ChatDbContext _context;

    public ChatRepository(ChatDbContext context)
    {
        _context = context;
    }

    public async Task<Chat?> GetChatAsync(Guid sellerId, Guid buyerId, Guid productId)
    {
        return await _context.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.SellerId == sellerId && c.BuyerId == buyerId && c.ProductId == productId);
    }

    public async Task<Chat?> GetChatByIdAsync(Guid chatId)
    {
        return await _context.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId);
    }

    public async Task<List<Chat>> GetUserChatsAsync(Guid userId)
    {
        return await _context.Chats
            .Include(c => c.Messages)
            .Where(c => c.SellerId == userId || c.BuyerId == userId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();
    }

    public async Task<Chat> CreateChatAsync(Chat chat)
    {
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();
        return chat;
    }

    public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task UpdateChatLastMessageTimeAsync(Guid chatId)
    {
        var chat = await _context.Chats.FindAsync(chatId);
        if (chat != null)
        {
            chat.LastMessageAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkMessagesAsReadAsync(Guid chatId, Guid userId)
    {
        var messages = await _context.ChatMessages
            .Where(m => m.ChatId == chatId && m.SenderId != userId && !m.IsRead)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<ChatMessage>> GetChatMessagesAsync(Guid chatId, int page, int pageSize)
    {
        return await _context.ChatMessages
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }
}