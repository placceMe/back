using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ChatService.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations<T>(this WebApplication app) where T : DbContext
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<T>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();

        try
        {
            // Отримуємо connection string і створюємо схему
            var connectionString = context.Database.GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is null or empty");
            }

            // Створюємо окреме з'єднання для створення схеми
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var schemaName = "chat_service";

            // Тимчасово прибираємо Search Path для створення схеми
            var originalSearchPath = builder.SearchPath;
            builder.SearchPath = "";

            using (var connection = new NpgsqlConnection(builder.ToString()))
            {
                connection.Open();
                using var command = new NpgsqlCommand($"CREATE SCHEMA IF NOT EXISTS {schemaName};", connection);
                command.ExecuteNonQuery();
                logger.LogInformation("Schema '{SchemaName}' created or already exists", schemaName);

                // Перевіряємо чи існує таблиця міграцій в неправильному місці і переносимо її
                using var checkCommand = new NpgsqlCommand(@"
                    SELECT EXISTS (
                        SELECT FROM information_schema.tables 
                        WHERE table_schema = 'public' 
                        AND table_name = '__EFMigrationsHistory'
                    )", connection);
                var existsInPublic = (bool)checkCommand.ExecuteScalar()!;

                if (existsInPublic)
                {
                    logger.LogInformation("Moving existing __EFMigrationsHistory from public to chat_service schema");
                    using var moveCommand = new NpgsqlCommand($@"
                        ALTER TABLE public.__EFMigrationsHistory SET SCHEMA {schemaName};", connection);
                    moveCommand.ExecuteNonQuery();
                }
            }

            // Застосовуємо міграції з правильним Search Path
            context.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying migrations");
            throw;
        }
    }
}