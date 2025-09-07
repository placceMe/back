using Marketplace.Authentication.Extensions;
using Marketplace.Authentication.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ??????? shared authentication - ?????? ???? ?????!
builder.Services.AddMarketplaceAuthentication(builder.Configuration, "products-service");

// ???? ???????
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS ??? ????????? cookies
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins("http://localhost:3000", "https://yourdomain.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // ??????? ??? cookies
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

// Shared authentication middleware
app.UseAuthentication();
app.UseMiddleware<SessionActivityMiddleware>(); // ??????????? ????????? ?????
app.UseAuthorization();

app.MapControllers();

app.Run();