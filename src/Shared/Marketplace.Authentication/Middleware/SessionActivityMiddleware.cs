using Marketplace.Authentication.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Marketplace.Authentication.Middleware;

public class SessionActivityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionActivityMiddleware> _logger;

    public SessionActivityMiddleware(RequestDelegate next, ILogger<SessionActivityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISessionManager sessionManager)
    {
        try
        {
            // ????????? ?????????? ??? ???????????????? ????????????
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var sessionId = context.User.FindFirst("session_id")?.Value;
                if (!string.IsNullOrEmpty(sessionId))
                {
                    // ?????????? ????????? ?????????? (?? ???????? ?????)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await sessionManager.UpdateSessionActivityAsync(sessionId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to update session activity for {SessionId}", sessionId);
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SessionActivityMiddleware");
        }

        await _next(context);
    }
}