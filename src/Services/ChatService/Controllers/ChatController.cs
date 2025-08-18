using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ChatService.DTOs;
using ChatService.Hubs;
using ChatService.Services;
using ChatService.HttpClients;
using System.Security.Claims;

namespace ChatService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IUsersServiceClient _usersServiceClient;
    private readonly IProductsServiceClient _productsServiceClient;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(
        IChatService chatService,
        IUsersServiceClient usersServiceClient,
        IProductsServiceClient productsServiceClient,
        IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _usersServiceClient = usersServiceClient;
        _productsServiceClient = productsServiceClient;
        _hubContext = hubContext;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartChat([FromBody] StartChatRequest request)
    {
        var buyerId = GetUserId();

        // Перевірка що продукт існує і отримання sellerId
        var product = await _productsServiceClient.GetProductByIdAsync(request.ProductId);
        if (product == null)
            return NotFound("Product not found");

        var chat = await _chatService.CreateOrGetChatAsync(product.SellerId, buyerId, request.ProductId);

        return Ok(new { ChatId = chat.Id });
    }

    [HttpPost("{chatId}/messages")]
    public async Task<IActionResult> SendMessage(Guid chatId, [FromBody] SendMessageRequest request)
    {
        var userId = GetUserId();

        // Перевірка доступу до чату
        var chat = await _chatService.GetChatByIdAsync(chatId);
        if (chat == null)
            return NotFound("Chat not found");

        if (chat.SellerId != userId && chat.BuyerId != userId)
            return Forbid("Access denied to this chat");

        var message = await _chatService.SendMessageAsync(chatId, userId, request.Content);

        if (message != null)
        {
            // Відправка через SignalR
            await _hubContext.Clients.Group($"Chat_{chatId}").SendAsync("ReceiveMessage", new
            {
                Id = message.Id,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead
            });

            return Ok(new
            {
                Id = message.Id,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead
            });
        }

        return BadRequest("Failed to send message");
    }

    [HttpGet("my-chats")]
    public async Task<IActionResult> GetMyChats()
    {
        var userId = GetUserId();
        var chats = await _chatService.GetUserChatsAsync(userId);

        var chatDtos = new List<object>();

        foreach (var chat in chats)
        {
            var otherUserId = chat.SellerId == userId ? chat.BuyerId : chat.SellerId;
            var otherUser = await _usersServiceClient.GetUserByIdAsync(otherUserId);
            var product = await _productsServiceClient.GetProductByIdAsync(chat.ProductId);

            chatDtos.Add(new
            {
                ChatId = chat.Id,
                OtherUser = new { Id = otherUser?.Id ?? Guid.Empty, Name = otherUser?.Name ?? "Unknown" },
                Product = new { Id = product?.Id ?? Guid.Empty, Title = product?.Title ?? "Unknown" },
                LastMessageAt = chat.LastMessageAt,
                UnreadCount = chat.Messages.Count(m => m.SenderId != userId && !m.IsRead),
                Role = chat.SellerId == userId ? "seller" : "buyer"
            });
        }

        return Ok(chatDtos);
    }

    [HttpGet("{chatId}")]
    public async Task<IActionResult> GetChat(Guid chatId)
    {
        var userId = GetUserId();
        var chat = await _chatService.GetChatByIdAsync(chatId);

        if (chat == null)
            return NotFound("Chat not found");

        if (chat.SellerId != userId && chat.BuyerId != userId)
            return Forbid("Access denied to this chat");

        var otherUserId = chat.SellerId == userId ? chat.BuyerId : chat.SellerId;
        var otherUser = await _usersServiceClient.GetUserByIdAsync(otherUserId);
        var product = await _productsServiceClient.GetProductByIdAsync(chat.ProductId);

        return Ok(new
        {
            ChatId = chat.Id,
            OtherUser = new { Id = otherUser?.Id ?? Guid.Empty, Name = otherUser?.Name ?? "Unknown" },
            Product = new { Id = product?.Id ?? Guid.Empty, Title = product?.Title ?? "Unknown" },
            Role = chat.SellerId == userId ? "seller" : "buyer"
        });
    }

    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetChatMessages(Guid chatId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userId = GetUserId();
        var chat = await _chatService.GetChatByIdAsync(chatId);

        if (chat == null)
            return NotFound("Chat not found");

        if (chat.SellerId != userId && chat.BuyerId != userId)
            return Forbid("Access denied to this chat");

        var messages = await _chatService.GetChatMessagesAsync(chatId, page, pageSize);
        return Ok(messages);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID claim not found");

        return Guid.Parse(userIdClaim);
    }
}