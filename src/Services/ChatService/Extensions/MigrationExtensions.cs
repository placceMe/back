using Microsoft.EntityFrameworkCore;

namespace ChatService.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations<T>(this WebApplication app) where T : DbContext
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<T>();

        try
        {
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();
            logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }
}