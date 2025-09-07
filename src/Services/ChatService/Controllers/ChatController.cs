using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ChatService.Data;
using ChatService.Models;
using ChatService.DTOs;
using ChatService.Hubs;
using ChatService.Services;
using ChatService.Extensions;
using Marketplace.Contracts.Chat;
using Marketplace.Contracts.Common;

namespace ChatService.Controllers
{
    /// <summary>
    /// API для управління чатами між покупцями та продавцями
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ChatsController : ControllerBase
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<ChatsController> _logger;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ProductsServiceClient _productsServiceClient;
        private readonly INotificationService _notificationService;

        public ChatsController(
            ChatDbContext context, 
            ILogger<ChatsController> logger, 
            IHubContext<ChatHub> hubContext,
            ProductsServiceClient productsServiceClient,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
            _productsServiceClient = productsServiceClient;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Створює новий чат або повертає існуючий
        /// </summary>
        /// <param name="request">Дані для створення чату</param>
        /// <returns>Інформація про чат</returns>
        /// <response code="200">Чат успішно створено або знайдено існуючий</response>
        /// <response code="400">Невалідні дані запиту або продавець не є власником товару</response>
        /// <response code="500">Внутрішня помилка сервера</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ChatDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ChatDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<ChatDto>), 500)]
        public async Task<ActionResult<ApiResponse<ChatDto>>> CreateOrGetChat(CreateChatDto request)
        {
            try
            {
                // Мапінг з контракту до локального DTO
                var localRequest = request.ToLocal();

                // Валідація відповідності sellerId продавцю товару productId
                var validation = await _productsServiceClient.ValidateProductSellerAsync(request.ProductId, request.SellerId);
                if (!validation.IsValid)
                {
                    return BadRequest(ApiResponse<ChatDto>.ErrorResult(validation.Error ?? "Невалідний продавець для товару"));
                }

                // Check if chat already exists
                var existingChat = await _context.Chats
                    .FirstOrDefaultAsync(c => c.ProductId == request.ProductId && 
                                            c.SellerId == request.SellerId && 
                                            c.BuyerId == request.BuyerId);

                if (existingChat != null)
                {
                    _logger.LogInformation("Returning existing chat {ChatId} for product {ProductId}", 
                        existingChat.Id, request.ProductId);
                    
                    return Ok(ApiResponse<ChatDto>.SuccessResult(existingChat.ToContract(), "Existing chat found"));
                }

                // Create new chat
                var newChat = new Chat
                {
                    Id = Guid.NewGuid(),
                    ProductId = request.ProductId,
                    SellerId = request.SellerId,
                    BuyerId = request.BuyerId
                };

                _context.Chats.Add(newChat);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new chat {ChatId} for product {ProductId}", 
                    newChat.Id, request.ProductId);

                return Ok(ApiResponse<ChatDto>.SuccessResult(newChat.ToContract(), "Chat created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/getting chat for product {ProductId}", request.ProductId);
                return StatusCode(500, ApiResponse<ChatDto>.ErrorResult("Внутрішня помилка сервера"));
            }
        }

        /// <summary>
        /// Отримує список чатів з фільтрацією та пагінацією
        /// </summary>
        /// <param name="sellerId">ID продавця для фільтрації</param>
        /// <param name="buyerId">ID покупця для фільтрації</param>
        /// <param name="skip">Кількість чатів для пропуску</param>
        /// <param name="take">Кількість чатів для повернення</param>
        /// <returns>Список чатів</returns>
        /// <response code="200">Успішно отримано список чатів</response>
        /// <response code="500">Внутрішня помилка сервера</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ChatDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ChatDto>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ChatDto>>>> GetChats(
            [FromQuery] Guid? sellerId,
            [FromQuery] Guid? buyerId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20)
        {
            try
            {
                var queryable = _context.Chats.AsQueryable();

                if (skip > 0)
                {
                    queryable = queryable.Skip(skip);
                }
                        
                // Фільтрація за sellerId або buyerId
                if (sellerId.HasValue)
                {
                    queryable = queryable.Where(c => c.SellerId == sellerId.Value);
                }

                if (buyerId.HasValue)
                {
                    queryable = queryable.Where(c => c.BuyerId == buyerId.Value);
                }

                var chats = await queryable
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(take)
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<ChatDto>>.SuccessResult(chats.ToContract()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chats");
                return StatusCode(500, ApiResponse<IEnumerable<ChatDto>>.ErrorResult("Внутрішня помилка сервера"));
            }
        }

        /// <summary>
        /// Отримує деталі конкретного чату
        /// </summary>
        /// <param name="chatId">ID чату</param>
        /// <returns>Деталі чату</returns>
        /// <response code="200">Чат знайдено</response>
        /// <response code="404">Чат не знайдено</response>
        /// <response code="500">Внутрішня помилка сервера</response>
        [HttpGet("{chatId}")]
        [ProducesResponseType(typeof(ApiResponse<ChatDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ChatDto>), 404)]
        [ProducesResponseType(typeof(ApiResponse<ChatDto>), 500)]
        public async Task<ActionResult<ApiResponse<ChatDto>>> GetChat(Guid chatId)
        {
            try
            {
                var chat = await _context.Chats.FindAsync(chatId);
                
                if (chat == null)
                {
                    return NotFound(ApiResponse<ChatDto>.ErrorResult("Чат не знайдено"));
                }

                return Ok(ApiResponse<ChatDto>.SuccessResult(chat.ToContract()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat {ChatId}", chatId);
                return StatusCode(500, ApiResponse<ChatDto>.ErrorResult("Внутрішня помилка сервера"));
            }
        }

        /// <summary>
        /// Отримує повідомлення чату
        /// </summary>
        /// <param name="chatId">ID чату</param>
        /// <param name="after">Дата після якої отримувати повідомлення</param>
        /// <param name="skip">Кількість повідомлень для пропуску</param>
        /// <param name="take">Кількість повідомлень для повернення</param>
        /// <returns>Список повідомлень</returns>
        /// <response code="200">Успішно отримано повідомлення</response>
        /// <response code="404">Чат не знайдено</response>
        /// <response code="500">Внутрішня помилка сервера</response>
        [HttpGet("{chatId}/messages")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ChatMessageDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ChatMessageDto>>), 404)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ChatMessageDto>>), 500)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ChatMessageDto>>>> GetMessages(
            Guid chatId,
            [FromQuery] DateTime? after = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                // Check if chat exists
                var chatExists = await _context.Chats.AnyAsync(c => c.Id == chatId);
                if (!chatExists)
                {
                    return NotFound(ApiResponse<IEnumerable<ChatMessageDto>>.ErrorResult("Чат не знайдено"));
                }

                var queryable = _context.ChatMessages
                    .Where(m => m.ChatId == chatId)
                    .AsQueryable();

                // Фільтрація за датою якщо вказано
                if (after.HasValue)
                {
                    queryable = queryable.Where(m => m.CreatedAt > after.Value);
                }

                var messages = await queryable
                    .OrderBy(m => m.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<ChatMessageDto>>.SuccessResult(messages.ToContract()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for chat {ChatId}", chatId);
                return StatusCode(500, ApiResponse<IEnumerable<ChatMessageDto>>.ErrorResult("Внутрішня помилка сервера"));
            }
        }

        /// <summary>
        /// Відправляє повідомлення в чат
        /// </summary>
        /// <param name="chatId">ID чату</param>
        /// <param name="request">Дані повідомлення</param>
        /// <returns>Створене повідомлення</returns>
        /// <response code="200">Повідомлення успішно відправлено</response>
        /// <response code="400">Невалідні дані повідомлення</response>
        /// <response code="404">Чат не знайдено</response>
        /// <response code="500">Внутрішня помилка сервера</response>
        [HttpPost("{chatId}/messages")]
        [ProducesResponseType(typeof(ApiResponse<ChatMessageDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ChatMessageDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<ChatMessageDto>), 404)]
        [ProducesResponseType(typeof(ApiResponse<ChatMessageDto>), 500)]
        public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendMessage(Guid chatId, CreateMessageDto request)
        {
            try
            {
                // Мапінг з контракту до локального DTO
                var localRequest = request.ToLocal();

                // Validate body is not empty/whitespace
                if (string.IsNullOrWhiteSpace(request.Body))
                {
                    return BadRequest(ApiResponse<ChatMessageDto>.ErrorResult("Текст повідомлення не може бути порожнім"));
                }

                // Check if chat exists and get chat info
                var chat = await _context.Chats.FindAsync(chatId);
                if (chat == null)
                {
                    return NotFound(ApiResponse<ChatMessageDto>.ErrorResult("Чат не знайдено"));
                }

                var message = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    ChatId = chatId,
                    SenderUserId = request.SenderUserId,
                    Body = request.Body.Trim()
                };

                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created message {MessageId} in chat {ChatId} by user {UserId}", 
                    message.Id, chatId, request.SenderUserId);

                var contractMessage = message.ToContract();

                // Send SignalR notification to chat group
                await _hubContext.Clients.Group($"chat:{chatId}")
                    .SendAsync("MessageCreated", contractMessage);

                _logger.LogInformation("Sent SignalR notification for message {MessageId} to group chat:{ChatId}", 
                    message.Id, chatId);

                // Send notification to users
                await _notificationService.SendMessageNotificationAsync(message, chat);

                return Ok(ApiResponse<ChatMessageDto>.SuccessResult(contractMessage, "Message sent successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chat {ChatId}", chatId);
                return StatusCode(500, ApiResponse<ChatMessageDto>.ErrorResult("Внутрішня помилка сервера"));
            }
        }
    }
}