using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ChatService.Data;
using ChatService.Models;
using ChatService.DTOs;
using ChatService.Hubs;
using ChatService.Services;

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
        // If you intended to use a cursor-based pagination, add this property to GetChatsQuery:
        public DateTime? Cursor { get; set; }
        /// <summary>
        /// Створює новий чат або повертає існуючий
        /// </summary>
        /// <param name="request">Дані для створення чату</param>
        /// <returns>Інформація про чат</returns>
        /// <response code="200">Чат успішно створено або знайдено існуючий</response>
        /// <response code="400">Невалідні дані запиту або продавець не є власником товару</response>
        /// <response code="500">Внутрішня помилка сервера</response>
        [HttpPost]
        [ProducesResponseType(typeof(ChatResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<ActionResult<ChatResponse>> CreateOrGetChat(CreateChatRequest request)
        {
            try
            {
                // Валідація відповідності sellerId продавцю товару productId
                var validation = await _productsServiceClient.ValidateProductSellerAsync(request.ProductId, request.SellerId);
                if (!validation.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Code = ErrorCodes.InvalidSeller,
                        Message = validation.Error ?? "Невалідний продавець для товару",
                        Details = new { ProductId = request.ProductId, SellerId = request.SellerId }
                    });
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
                    
                    return Ok(new ChatResponse
                    {
                        Id = existingChat.Id,
                        ProductId = existingChat.ProductId,
                        SellerId = existingChat.SellerId,
                        BuyerId = existingChat.BuyerId,
                        CreatedAt = existingChat.CreatedAt
                    });
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

                return Ok(new ChatResponse
                {
                    Id = newChat.Id,
                    ProductId = newChat.ProductId,
                    SellerId = newChat.SellerId,
                    BuyerId = newChat.BuyerId,
                    CreatedAt = newChat.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/getting chat for product {ProductId}", request.ProductId);
                return StatusCode(500, new ErrorResponse
                {
                    Code = ErrorCodes.InternalError,
                    Message = "Внутрішня помилка сервера",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Отримує список чатів з фільтрацією та пагінацією
        /// </summary>
        /// <param name="query">Параметри фільтрації та пагінації</param>
        /// <returns>Список чатів</returns>
        /// <response code="200">Успішно отримано список чатів</response>
        /// <response code="500">Внутрішня помилка сервера</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ChatResponse>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<ActionResult<IEnumerable<ChatResponse>>> GetChats([FromQuery] GetChatsQuery query)
        {
            try
            {
                var queryable = _context.Chats.AsQueryable();
                // Replace this block in GetChats method:
                // if (query.Cursor.HasValue)
                // {
                //     queryable = queryable.Where(c => c.CreatedAt < query.Cursor.Value);
                // }

                // With this block:
                if (query.Skip > 0)
                {
                    queryable = queryable.Skip(query.Skip);
                }
                        
                // Фільтрація за sellerId або buyerId
                if (query.SellerId.HasValue)
                {
                    queryable = queryable.Where(c => c.SellerId == query.SellerId.Value);
                }

                if (query.BuyerId.HasValue)
                {
                    queryable = queryable.Where(c => c.BuyerId == query.BuyerId.Value);
                }


                var chats = await queryable
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                // Формування результату
                var chatResponses = chats.Select(c => new ChatResponse
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    SellerId = c.SellerId,
                    BuyerId = c.BuyerId,
                    CreatedAt = c.CreatedAt
                });

                return Ok(chatResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chats");
                return StatusCode(500, new ErrorResponse
                {
                    Code = ErrorCodes.InternalError,
                    Message = "Внутрішня помилка сервера"
                });
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
        [ProducesResponseType(typeof(ChatResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<ActionResult<ChatResponse>> GetChat(Guid chatId)
        {
            try
            {
                var chat = await _context.Chats.FindAsync(chatId);
                
                if (chat == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Code = ErrorCodes.NotFound,
                        Message = "Чат не знайдено",
                        Details = new { ChatId = chatId }
                    });
                }

                return Ok(new ChatResponse
                {
                    Id = chat.Id,
                    ProductId = chat.ProductId,
                    SellerId = chat.SellerId,
                    BuyerId = chat.BuyerId,
                    CreatedAt = chat.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat {ChatId}", chatId);
                return StatusCode(500, new ErrorResponse
                {
                    Code = ErrorCodes.InternalError,
                    Message = "Внутрішня помилка сервера"
                });
            }
        }

        /// <summary>
        /// Отримує повідомлення чату
        /// </summary>
        /// <param name="chatId">ID чату</param>
        /// <param name="query">Параметри пагінації</param>
        /// <returns>Список повідомлень</returns>
        /// <response code="200">Успішно отримано повідомлення</response>
        /// <response code="404">Чат не знайдено</response>
        /// <response code="500">Внутрішня помилка сервера</response>
        [HttpGet("{chatId}/messages")]
        [ProducesResponseType(typeof(IEnumerable<MessageResponse>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<ActionResult<IEnumerable<MessageResponse>>> GetMessages(Guid chatId, [FromQuery] GetMessagesQuery query)
        {
            try
            {
                // Check if chat exists
                var chatExists = await _context.Chats.AnyAsync(c => c.Id == chatId);
                if (!chatExists)
                {
                    return NotFound(new ErrorResponse
                    {
                        Code = ErrorCodes.ChatNotFound,
                        Message = "Чат не знайдено",
                        Details = new { ChatId = chatId }
                    });
                }

                var queryable = _context.ChatMessages
                    .Where(m => m.ChatId == chatId)
                    .AsQueryable();

                // Фільтрація за датою якщо вказано
                if (query.After.HasValue)
                {
                    queryable = queryable.Where(m => m.CreatedAt > query.After.Value);
                }

                // Пагінація
                var totalMessages = await queryable.CountAsync();
                var messages = await queryable
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();

                var messageResponses = messages.Select(m => new MessageResponse
                {
                    Id = m.Id,
                    ChatId = m.ChatId,
                    SenderUserId = m.SenderUserId,
                    Body = m.Body,
                    CreatedAt = m.CreatedAt
                });

                return Ok(messageResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for chat {ChatId}", chatId);
                return StatusCode(500, new ErrorResponse
                {
                    Code = ErrorCodes.InternalError,
                    Message = "Внутрішня помилка сервера"
                });
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
        [ProducesResponseType(typeof(MessageResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<ActionResult<MessageResponse>> SendMessage(Guid chatId, CreateMessageRequest request)
        {
            try
            {
                // Validate body is not empty/whitespace
                if (string.IsNullOrWhiteSpace(request.Body))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Code = ErrorCodes.EmptyMessage,
                        Message = "Текст повідомлення не може бути порожнім",
                        Details = new { ChatId = chatId }
                    });
                }

                // Check if chat exists and get chat info
                var chat = await _context.Chats.FindAsync(chatId);
                if (chat == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Code = ErrorCodes.ChatNotFound,
                        Message = "Чат не знайдено",
                        Details = new { ChatId = chatId }
                    });
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

                var messageResponse = new MessageResponse
                {
                    Id = message.Id,
                    ChatId = message.ChatId,
                    SenderUserId = message.SenderUserId,
                    Body = message.Body,
                    CreatedAt = message.CreatedAt
                };

                // Send SignalR notification to chat group
                await _hubContext.Clients.Group($"chat:{chatId}")
                    .SendAsync("MessageCreated", messageResponse);

                _logger.LogInformation("Sent SignalR notification for message {MessageId} to group chat:{ChatId}", 
                    message.Id, chatId);

                // Send notification to users
                await _notificationService.SendMessageNotificationAsync(message, chat);

                return Ok(messageResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chat {ChatId}", chatId);
                return StatusCode(500, new ErrorResponse
                {
                    Code = ErrorCodes.InternalError,
                    Message = "Внутрішня помилка сервера"
                });
            }
        }
    }
}