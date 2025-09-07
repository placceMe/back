using System.ComponentModel.DataAnnotations;

namespace Marketplace.Contracts.Chat;

/// <summary>
/// Chat data transfer object
/// </summary>
public class ChatDto
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public Guid SellerId { get; set; }
    
    [Required]
    public Guid BuyerId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // ????????? ?????????? ??? UI
    public string ProductTitle { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public ChatMessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}

/// <summary>
/// Chat message DTO
/// </summary>
public class ChatMessageDto
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid ChatId { get; set; }
    
    [Required]
    public Guid SenderUserId { get; set; }
    
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Body { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Create chat request
/// </summary>
public class CreateChatDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public Guid SellerId { get; set; }
    
    [Required]
    public Guid BuyerId { get; set; }
}

/// <summary>
/// Create message request
/// </summary>
public class CreateMessageDto
{
    [Required]
    public Guid SenderUserId { get; set; }
    
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// Message notification for SignalR
/// </summary>
public class MessageNotificationDto
{
    public Guid ChatId { get; set; }
    public Guid ProductId { get; set; }
    public Guid SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string MessagePreview { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
}