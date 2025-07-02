using FilesService.Services;
using FilesService.Models;
using Minio;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Конфігурація MinIO  
builder.Services.Configure<MinIOConfig>(
   builder.Configuration.GetSection("MinIO"));

// Реєстрація MinIO клієнта  
builder.Services.AddSingleton<IMinioClient>(provider =>
{
    var config = provider.GetRequiredService<IOptions<MinIOConfig>>().Value;
    return new MinioClient()
        .WithEndpoint(config.Endpoint)
        .WithCredentials(config.AccessKey, config.SecretKey)
        .WithSSL(config.Secure)
        .Build();
});

// Реєстрація сервісів  
builder.Services.AddScoped<IFilesService, FilesService.Services.FilesService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Налаштування для великих файлів  
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100_000_000; // 100 MB  
});

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseRouting();
app.MapControllers();

app.Run();