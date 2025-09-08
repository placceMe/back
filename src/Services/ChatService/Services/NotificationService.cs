using Microsoft.AspNetCore.SignalR;
using ChatService.Hubs;
using Marketplace.Contracts.Chat;
using Marketplace.Contracts.Common;
using ChatService.Services;
using ChatService.Models;

namespace ChatService.Services
{
    public interface INotificationService
    {
        Task SendMessageNotificationAsync(ChatMessage message, Chat chat);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly UsersServiceClient _usersServiceClient;
        private readonly ProductsServiceClient _productsServiceClient;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<ChatHub> hubContext,
            UsersServiceClient usersServiceClient,
            ProductsServiceClient productsServiceClient,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _usersServiceClient = usersServiceClient;
            _productsServiceClient = productsServiceClient;
            _logger = logger;
        }

        public async Task SendMessageNotificationAsync(ChatMessage message, Chat chat)
        {
            try
            {
                // ????????? ?????????? ??? ???????????
                var senderInfo = await _usersServiceClient.GetUserInfoAsync(message.SenderUserId);
                if (senderInfo == null)
                {
                    _logger.LogWarning("Could not get sender info for user {UserId}", message.SenderUserId);
                    senderInfo = new UserInfo { Id = message.SenderUserId, Name = "Unknown User" };
                }

                // ????????? ?????????? ??? ?????
                var productInfo = await _productsServiceClient.GetProductInfoAsync(chat.ProductId);

                // ????????? ???????????
                var notification = new MessageNotificationDto
                {
                    ChatId = message.ChatId,
                    ProductId = chat.ProductId,
                    SenderUserId = message.SenderUserId,
                    SenderName = senderInfo.Name,
                    MessagePreview = TruncateMessage(message.Body, 100),
                    CreatedAt = message.CreatedAt,
                    ProductTitle = productInfo?.Title ?? "Unknown Product",
                    ProductImageUrl = productInfo?.MainImageUrl
                };

                // ?????????? ??? ??? ???????? ??????????? (??? ???????? ???? ???????????)
                var recipientIds = new List<Guid>();

                if (chat.SellerId != message.SenderUserId)
                {
                    recipientIds.Add(chat.SellerId);
                }

                if (chat.BuyerId != message.SenderUserId)
                {
                    recipientIds.Add(chat.BuyerId);
                }

                // ???????????? ??????????? ????????????
                foreach (var recipientId in recipientIds)
                {
                    var userGroupName = $"user:{recipientId}";
                    await _hubContext.Clients.Group(userGroupName)
                        .SendAsync("MessageNotification", notification);

                    _logger.LogInformation("Sent message notification to user {UserId} for message {MessageId}",
                        recipientId, message.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message notification for message {MessageId}", message.Id);
            }
        }

        private string TruncateMessage(string message, int maxLength)
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            if (message.Length <= maxLength)
                return message;

            return message.Substring(0, maxLength - 3) + "...";
        }
    }
}
