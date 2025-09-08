using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ChatService.Data;

namespace ChatService.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ChatDbContext context, ILogger<ChatHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Join a chat group to receive real-time messages
        /// </summary>
        /// <param name="chatId">The chat ID to join</param>
        public async Task JoinChat(string chatId)
        {
            try
            {
                // Validate chat exists
                if (!Guid.TryParse(chatId, out var chatGuid))
                {
                    _logger.LogWarning("Invalid chat ID format: {ChatId}", chatId);
                    await Clients.Caller.SendAsync("Error", "Invalid chat ID format");
                    return;
                }

                var chatExists = await _context.Chats.AnyAsync(c => c.Id == chatGuid);
                if (!chatExists)
                {
                    _logger.LogWarning("Attempt to join non-existent chat: {ChatId}", chatId);
                    await Clients.Caller.SendAsync("Error", "Chat not found");
                    return;
                }

                var groupName = $"chat:{chatId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                
                _logger.LogInformation("Connection {ConnectionId} joined chat group {GroupName}", 
                    Context.ConnectionId, groupName);

                await Clients.Caller.SendAsync("JoinedChat", chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining chat {ChatId} for connection {ConnectionId}", 
                    chatId, Context.ConnectionId);
                await Clients.Caller.SendAsync("Error", "Failed to join chat");
            }
        }

        /// <summary>
        /// Leave a chat group
        /// </summary>
        /// <param name="chatId">The chat ID to leave</param>
        public async Task LeaveChat(string chatId)
        {
            try
            {
                var groupName = $"chat:{chatId}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                
                _logger.LogInformation("Connection {ConnectionId} left chat group {GroupName}", 
                    Context.ConnectionId, groupName);

                await Clients.Caller.SendAsync("LeftChat", chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving chat {ChatId} for connection {ConnectionId}", 
                    chatId, Context.ConnectionId);
                await Clients.Caller.SendAsync("Error", "Failed to leave chat");
            }
        }

        /// <summary>
        /// Subscribe user to receive notifications about new messages
        /// </summary>
        /// <param name="userId">The user ID to subscribe for notifications</param>
        public async Task SubscribeToUserNotifications(string userId)
        {
            try
            {
                // Validate user ID format
                if (!Guid.TryParse(userId, out var userGuid))
                {
                    _logger.LogWarning("Invalid user ID format: {UserId}", userId);
                    await Clients.Caller.SendAsync("Error", "Invalid user ID format");
                    return;
                }

                var userGroupName = $"user:{userId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, userGroupName);
                
                _logger.LogInformation("Connection {ConnectionId} subscribed to user notifications {UserGroupName}", 
                    Context.ConnectionId, userGroupName);

                await Clients.Caller.SendAsync("SubscribedToNotifications", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to user notifications {UserId} for connection {ConnectionId}", 
                    userId, Context.ConnectionId);
                await Clients.Caller.SendAsync("Error", "Failed to subscribe to notifications");
            }
        }

        /// <summary>
        /// Unsubscribe user from notifications
        /// </summary>
        /// <param name="userId">The user ID to unsubscribe from notifications</param>
        public async Task UnsubscribeFromUserNotifications(string userId)
        {
            try
            {
                var userGroupName = $"user:{userId}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userGroupName);
                
                _logger.LogInformation("Connection {ConnectionId} unsubscribed from user notifications {UserGroupName}", 
                    Context.ConnectionId, userGroupName);

                await Clients.Caller.SendAsync("UnsubscribedFromNotifications", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from user notifications {UserId} for connection {ConnectionId}", 
                    userId, Context.ConnectionId);
                await Clients.Caller.SendAsync("Error", "Failed to unsubscribe from notifications");
            }
        }

        /// <summary>
        /// Handle client disconnection
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Connection {ConnectionId} disconnected", Context.ConnectionId);
            
            if (exception != null)
            {
                _logger.LogWarning(exception, "Connection {ConnectionId} disconnected with error", 
                    Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Handle client connection
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Connection {ConnectionId} connected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }
    }
}
