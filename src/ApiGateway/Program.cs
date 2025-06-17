using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Додаємо YARP + конфіг з appsettings
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy(); // головна точка входу

app.Run();
