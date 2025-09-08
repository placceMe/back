using ChatService.Models;
using ContractChatDto = Marketplace.Contracts.Chat.ChatDto;
using ContractChatMessageDto = Marketplace.Contracts.Chat.ChatMessageDto;
using ContractCreateChatDto = Marketplace.Contracts.Chat.CreateChatDto;
using ContractCreateMessageDto = Marketplace.Contracts.Chat.CreateMessageDto;
using ContractMessageNotificationDto = Marketplace.Contracts.Chat.MessageNotificationDto;

namespace ChatService.Extensions;

public static class ContractMappingExtensions
{
    // Chat model to contract mapping
    public static ContractChatDto ToContract(this Chat chat)
    {
        return new ContractChatDto
        {
            Id = chat.Id,
            ProductId = chat.ProductId,
            SellerId = chat.SellerId,
            BuyerId = chat.BuyerId,
            CreatedAt = chat.CreatedAt,
            ProductTitle = string.Empty, // Will be filled by service
            ProductImageUrl = string.Empty, // Will be filled by service
            LastMessage = null, // Will be filled by service
            UnreadCount = 0 // Will be filled by service
        };
    }

    // ChatMessage model to contract mapping
    public static ContractChatMessageDto ToContract(this ChatMessage message)
    {
        return new ContractChatMessageDto
        {
            Id = message.Id,
            ChatId = message.ChatId,
            SenderUserId = message.SenderUserId,
            Body = message.Body,
            CreatedAt = message.CreatedAt
        };
    }

    // Contract to local model mappings
    public static Chat ToLocal(this ContractCreateChatDto contractDto)
    {
        return new Chat
        {
            ProductId = contractDto.ProductId,
            SellerId = contractDto.SellerId,
            BuyerId = contractDto.BuyerId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ChatMessage ToLocal(this ContractCreateMessageDto contractDto)
    {
        return new ChatMessage
        {
            SenderUserId = contractDto.SenderUserId,
            Body = contractDto.Body,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Collection mappings
    public static IEnumerable<ContractChatDto> ToContract(this IEnumerable<Chat> chats)
    {
        return chats.Select(c => c.ToContract());
    }

    public static IEnumerable<ContractChatMessageDto> ToContract(this IEnumerable<ChatMessage> messages)
    {
        return messages.Select(m => m.ToContract());
    }
}
