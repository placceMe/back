using Microsoft.EntityFrameworkCore;

namespace ProductsService.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations<TContext>(this IApplicationBuilder app) where TContext : DbContext
        {
            using var scope = app.ApplicationServices.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            try
            {
                logger.LogInformation("Starting database migration...");

                // Ensure schema exists
                context.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS products_service;");

                // Apply pending migrations
                context.Database.Migrate();
                logger.LogInformation("Database migration completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Migration failed: {Message}", ex.Message);
                throw;
            }
        }
    }
}