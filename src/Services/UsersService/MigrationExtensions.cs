using Microsoft.EntityFrameworkCore;

namespace UsersService.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations<TContext>(this IApplicationBuilder app) where TContext : DbContext
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
            
            try
            {
                Console.WriteLine("Starting database migration...");
                dbContext.Database.Migrate();
                Console.WriteLine("Database migration completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration failed: {ex.Message}");
                throw;
            }
        }
        
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContexts = scope.ServiceProvider.GetServices<DbContext>();
            
            foreach (var dbContext in dbContexts)
            {
                try
                {
                    Console.WriteLine($"Starting database migration for {dbContext.GetType().Name}...");
                    dbContext.Database.Migrate();
                    Console.WriteLine($"Database migration completed successfully for {dbContext.GetType().Name}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Migration failed for {dbContext.GetType().Name}: {ex.Message}");
                    throw;
                }
            }
        }
    }
}