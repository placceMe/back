using Marketplace.Authentication.Abstractions;
using Marketplace.Authentication.Configuration;
using Marketplace.Authentication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Marketplace.Authentication.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarketplaceAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string? serviceName = null)
    {
        // ????????????
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));
        
        var authOptions = configuration.GetSection(AuthenticationOptions.SectionName).Get<AuthenticationOptions>()
            ?? throw new InvalidOperationException("Authentication configuration is missing");

        // Redis
        var redis = ConnectionMultiplexer.Connect(authOptions.Redis.ConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(redis);

        // Data Protection
        services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(redis, $"{authOptions.Redis.KeyPrefix}dp-keys");

        // Authentication Services
        services.AddSingleton<JwtTokenService>();
        services.AddSingleton<ISessionManager, RedisSessionManager>();

        // JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Jwt.SecretKey)),
                    ValidateIssuer = authOptions.Jwt.ValidateIssuer,
                    ValidIssuer = authOptions.Jwt.Issuer,
                    ValidateAudience = authOptions.Jwt.ValidateAudience,
                    ValidAudience = authOptions.Jwt.Audience,
                    ValidateLifetime = authOptions.Jwt.ValidateLifetime,
                    ClockSkew = authOptions.Jwt.ClockSkew
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // ????????? 1: HttpOnly cookie (???????? ??????)
                        if (context.Request.Cookies.TryGetValue("authToken", out var cookieToken))
                        {
                            context.Token = cookieToken;
                            return Task.CompletedTask;
                        }

                        // ????????? 2: Authorization header (fallback ??? API)
                        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) && 
                            authHeader.ToString().StartsWith("Bearer "))
                        {
                            context.Token = authHeader.ToString().Replace("Bearer ", "");
                        }

                        return Task.CompletedTask;
                    },
                    
                    OnTokenValidated = async context =>
                    {
                        var sessionManager = context.HttpContext.RequestServices.GetRequiredService<ISessionManager>();
                        
                        // Отримуємо токен з заголовка Authorization
                        var authHeader = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                        {
                            var token = authHeader.Replace("Bearer ", "");
                            var sessionValidation = await sessionManager.ValidateSessionAsync(token);
                            if (!sessionValidation.IsValid)
                            {
                                context.Fail(sessionValidation.ErrorMessage ?? "Session validation failed");
                            }
                        }
                    },

                    OnAuthenticationFailed = context =>
                    {
                        // ????????? ??????? ??????????????
                        var logger = context.HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<JwtBearerHandler>>();
                        logger?.LogWarning("JWT Authentication failed: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },

                    OnChallenge = context =>
                    {
                        // ???????? ??????? 401 ??????????
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            
                            var response = new { 
                                message = "Unauthorized", 
                                needsLogin = true,
                                timestamp = DateTime.UtcNow 
                            };
                            
                            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}