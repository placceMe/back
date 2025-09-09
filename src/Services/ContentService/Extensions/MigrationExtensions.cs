using Microsoft.EntityFrameworkCore;

namespace ContentService.Extensions
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
                logger.LogInformation("Starting database migration for ContentService...");

                // Check if database can be connected
                var canConnect = context.Database.CanConnect();
                logger.LogInformation("Database connection status: {CanConnect}", canConnect);

                if (!canConnect)
                {
                    logger.LogError("Cannot connect to database");
                    throw new InvalidOperationException("Cannot connect to database");
                }

                // Ensure schema exists
                logger.LogInformation("Creating schema if not exists...");
                context.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS content_service;");

                // Check pending migrations
                var pendingMigrations = context.Database.GetPendingMigrations().ToList();
                logger.LogInformation("Found {Count} pending migrations: {Migrations}",
                    pendingMigrations.Count, string.Join(", ", pendingMigrations));

                var appliedMigrations = context.Database.GetAppliedMigrations().ToList();
                logger.LogInformation("Found {Count} applied migrations: {Migrations}",
                    appliedMigrations.Count, string.Join(", ", appliedMigrations));

                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying pending migrations...");
                    context.Database.Migrate();
                    logger.LogInformation("Migrations applied successfully");
                }
                else
                {
                    logger.LogInformation("No pending migrations found");
                }

                // Verify table exists
                var tableExistsQuery = @"
                    SELECT COUNT(*) FROM information_schema.tables 
                    WHERE table_schema = 'content_service' AND table_name = 'Faqs'
                ";
                var connection = context.Database.GetDbConnection();
                using var command = connection.CreateCommand();
                command.CommandText = tableExistsQuery;

                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                var result = command.ExecuteScalar();
                var tableExists = Convert.ToInt32(result) > 0;
                logger.LogInformation("Faqs table exists: {TableExists}", tableExists);

                logger.LogInformation("Database migration for ContentService completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Migration failed for ContentService: {Message}", ex.Message);
                throw;
            }
        }
    }
}
