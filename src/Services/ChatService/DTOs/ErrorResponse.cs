namespace ChatService.DTOs
{
    public class ErrorResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Details { get; set; }
    }

    public static class ErrorCodes
    {
        public const string ValidationError = "VALIDATION_ERROR";
        public const string NotFound = "NOT_FOUND";
        public const string InternalError = "INTERNAL_ERROR";
        public const string InvalidSeller = "INVALID_SELLER";
        public const string EmptyMessage = "EMPTY_MESSAGE";
        public const string ChatNotFound = "CHAT_NOT_FOUND";
    }
}