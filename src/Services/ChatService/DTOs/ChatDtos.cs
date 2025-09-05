using System.ComponentModel.DataAnnotations;

namespace ChatService.DTOs
{
    public class CreateChatRequest
    {
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        public Guid SellerId { get; set; }
        
        [Required]
        public Guid BuyerId { get; set; }
    }

    public class ChatResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid SellerId { get; set; }
        public Guid BuyerId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMessageRequest
    {
        [Required]
        public Guid SenderUserId { get; set; }
        
        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public string Body { get; set; } = string.Empty;
    }

    public class MessageResponse
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid SenderUserId { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class GetChatsQuery
    {
        public Guid? SellerId { get; set; }
        public Guid? BuyerId { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 20;
    }

    public class GetMessagesQuery
    {
        public DateTime? After { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
    }

    public class PagedResponse<T>
    {
        public int Total { get; set; }
        public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    }

    // ???? DTOs ??? ???????????
    public class MessageNotification
    {
        public Guid ChatId { get; set; }
        public Guid ProductId { get; set; }
        public Guid SenderUserId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string MessagePreview { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
    }

    public class ChatParticipantInfo
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public string Role { get; set; } = string.Empty; // "seller" or "buyer"
    }
}