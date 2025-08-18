using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ChatService.Services;
using System.Security.Claims;

namespace ChatService.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public async Task JoinChat(Guid chatId)
    {
        var userId = GetUserId();
        var chat = await _chatService.GetChatByIdAsync(chatId);

        if (chat != null && (chat.SellerId == userId || chat.BuyerId == userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
            _logger.LogInformation($"User {userId} joined chat {chatId}");
        }
    }

    public async Task LeaveChat(Guid chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
    }

    public async Task MarkAsRead(Guid chatId)
    {
        var userId = GetUserId();

        // Перевірка доступу до чату
        var chat = await _chatService.GetChatByIdAsync(chatId);
        if (chat == null || (chat.SellerId != userId && chat.BuyerId != userId))
        {
            _logger.LogWarning($"User {userId} attempted to mark messages as read in unauthorized chat {chatId}");
            return;
        }

        await _chatService.MarkMessagesAsReadAsync(chatId, userId);

        // Повідомити всіх учасників чату про зміну статусу
        await Clients.Group($"Chat_{chatId}").SendAsync("MessagesRead", new
        {
            ChatId = chatId,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation($"User {userId} marked messages as read in chat {chatId}");
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID claim not found");

        return Guid.Parse(userIdClaim);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        _logger.LogInformation($"User {userId} connected to chat hub");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        _logger.LogInformation($"User {userId} disconnected from chat hub");
        await base.OnDisconnectedAsync(exception);
    }
}