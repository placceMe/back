namespace ChatService.DTOs;

public record StartChatRequest(Guid ProductId);

public record SendMessageRequest(string Content);

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}

public class ChatDto
{
    public Guid Id { get; set; }
    public UserDto OtherUser { get; set; } = null!;
    public ProductDto Product { get; set; } = null!;
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public string Role { get; set; } = string.Empty; // "seller" or "buyer"
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid SellerId { get; set; }
    public decimal Price { get; set; }
}