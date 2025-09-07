namespace Marketplace.Authentication.Configuration;

public class AuthenticationOptions
{
    public const string SectionName = "Authentication";
    
    public JwtOptions Jwt { get; set; } = new();
    public RedisOptions Redis { get; set; } = new();
    public SessionOptions Session { get; set; } = new();
}

public class JwtOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;
}

public class RedisOptions
{
    public string ConnectionString { get; set; } = "redis:6379";
    public string KeyPrefix { get; set; } = "auth:";
    public int DefaultDatabaseIndex { get; set; } = 0;
}

public class SessionOptions
{
    public TimeSpan DefaultSessionDuration { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan RefreshThreshold { get; set; } = TimeSpan.FromMinutes(30);
    public int MaxConcurrentSessions { get; set; } = 5;
    public bool EnableSessionExtension { get; set; } = true;
    public TimeSpan SessionExtensionThreshold { get; set; } = TimeSpan.FromMinutes(15);
}