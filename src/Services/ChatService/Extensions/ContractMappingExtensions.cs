using ChatService.Models;
using ContractChatDto = Marketplace.Contracts.Chat.ChatDto;
using ContractChatMessageDto = Marketplace.Contracts.Chat.ChatMessageDto;
using ContractCreateChatDto = Marketplace.Contracts.Chat.CreateChatDto;
using ContractCreateMessageDto = Marketplace.Contracts.Chat.CreateMessageDto;
using ContractMessageNotificationDto = Marketplace.Contracts.Chat.MessageNotificationDto;
using ContractApiResponse = Marketplace.Contracts.Common.ApiResponse;
using LocalChatResponse = ChatService.DTOs.ChatResponse;
using LocalMessageResponse = ChatService.DTOs.MessageResponse;
using LocalCreateChatRequest = ChatService.DTOs.CreateChatRequest;
using LocalCreateMessageRequest = ChatService.DTOs.CreateMessageRequest;
using LocalMessageNotification = ChatService.DTOs.MessageNotification;

namespace ChatService.Extensions;

public static class ContractMappingExtensions
{
    // Chat mappings
    public static ContractChatDto ToContract(this LocalChatResponse localResponse)
    {
        return new ContractChatDto
        {
            Id = localResponse.Id,
            ProductId = localResponse.ProductId,
            SellerId = localResponse.SellerId,
            BuyerId = localResponse.BuyerId,
            CreatedAt = localResponse.CreatedAt,
            ProductTitle = string.Empty, // Will be populated from external service
            ProductImageUrl = string.Empty,
            LastMessage = null,
            UnreadCount = 0
        };
    }

    public static ContractChatDto ToContract(this Chat chat)
    {
        return new ContractChatDto
        {
            Id = chat.Id,
            ProductId = chat.ProductId,
            SellerId = chat.SellerId,
            BuyerId = chat.BuyerId,
            CreatedAt = chat.CreatedAt,
            ProductTitle = string.Empty,
            ProductImageUrl = string.Empty,
            LastMessage = null,
            UnreadCount = 0
        };
    }

    public static LocalCreateChatRequest ToLocal(this ContractCreateChatDto contractDto)
    {
        return new LocalCreateChatRequest
        {
            ProductId = contractDto.ProductId,
            SellerId = contractDto.SellerId,
            BuyerId = contractDto.BuyerId
        };
    }

    // Message mappings
    public static ContractChatMessageDto ToContract(this LocalMessageResponse localResponse)
    {
        return new ContractChatMessageDto
        {
            Id = localResponse.Id,
            ChatId = localResponse.ChatId,
            SenderUserId = localResponse.SenderUserId,
            Body = localResponse.Body,
            CreatedAt = localResponse.CreatedAt
        };
    }

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

    public static LocalCreateMessageRequest ToLocal(this ContractCreateMessageDto contractDto)
    {
        return new LocalCreateMessageRequest
        {
            SenderUserId = contractDto.SenderUserId,
            Body = contractDto.Body
        };
    }

    // Notification mappings
    public static ContractMessageNotificationDto ToContract(this LocalMessageNotification localNotification)
    {
        return new ContractMessageNotificationDto
        {
            ChatId = localNotification.ChatId,
            ProductId = localNotification.ProductId,
            SenderUserId = localNotification.SenderUserId,
            SenderName = localNotification.SenderName,
            MessagePreview = localNotification.MessagePreview,
            CreatedAt = localNotification.CreatedAt,
            ProductTitle = localNotification.ProductTitle,
            ProductImageUrl = localNotification.ProductImageUrl ?? string.Empty
        };
    }

    // Collection mappings
    public static IEnumerable<ContractChatDto> ToContract(this IEnumerable<LocalChatResponse> localResponses)
    {
        return localResponses.Select(r => r.ToContract());
    }

    public static IEnumerable<ContractChatDto> ToContract(this IEnumerable<Chat> chats)
    {
        return chats.Select(c => c.ToContract());
    }

    public static IEnumerable<ContractChatMessageDto> ToContract(this IEnumerable<LocalMessageResponse> localResponses)
    {
        return localResponses.Select(r => r.ToContract());
    }

    public static IEnumerable<ContractChatMessageDto> ToContract(this IEnumerable<ChatMessage> messages)
    {
        return messages.Select(m => m.ToContract());
    }
}