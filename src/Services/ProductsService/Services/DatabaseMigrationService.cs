using Microsoft.EntityFrameworkCore;


namespace ProductsService.Services;

public class DatabaseMigrationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(IServiceProvider serviceProvider, ILogger<DatabaseMigrationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Застосовує міграції для вказаного DbContext при старті додатку
    /// </summary>
    /// <typeparam name="TContext">Тип DbContext</typeparam>
    public async Task MigrateDatabaseAsync<TContext>() where TContext : DbContext
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            _logger.LogInformation("Перевіряємо підключення та міграції для {ContextName}...", typeof(TContext).Name);

            // Спочатку перевіряємо підключення до БД
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                _logger.LogError("Не вдається підключитися до бази даних для {ContextName}", typeof(TContext).Name);
                throw new InvalidOperationException($"Не вдається підключитися до бази даних для {typeof(TContext).Name}");
            }

            // Забезпечуємо створення бази даних (якщо не існує)
            var created = await context.Database.EnsureCreatedAsync();
            if (created)
            {
                _logger.LogInformation("База даних створена для {ContextName}", typeof(TContext).Name);
                return; // Якщо БД щойно створена, міграції не потрібні
            }

            // Перевіряємо чи потрібні міграції
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

            _logger.LogInformation("Для {ContextName}: застосовано {Applied} міграцій, очікується {Pending}",
                typeof(TContext).Name, appliedMigrations.Count(), pendingMigrations.Count());

            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Застосовуємо {Count} міграцій для {ContextName}: {Migrations}",
                    pendingMigrations.Count(),
                    typeof(TContext).Name,
                    string.Join(", ", pendingMigrations));

                // Застосовуємо міграції з retry логікою
                await RetryMigrateAsync(context, typeof(TContext).Name);

                _logger.LogInformation("Міграції для {ContextName} успішно застосовані!", typeof(TContext).Name);
            }
            else
            {
                _logger.LogInformation("Міграції для {ContextName} актуальні, пропускаємо.", typeof(TContext).Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при застосуванні міграцій для {ContextName}: {Message}", typeof(TContext).Name, ex.Message);

            // Якщо помилка пов'язана з тим, що таблиця вже існує, спробуємо продовжити
            if (ex.Message.Contains("already exists") || ex.Message.Contains("вже існує"))
            {
                _logger.LogWarning("Деякі об'єкти БД вже існують для {ContextName}, продовжуємо...", typeof(TContext).Name);
                return;
            }

            throw;
        }
    }

    /// <summary>
    /// Виконує міграції з retry логікою
    /// </summary>
    private async Task RetryMigrateAsync(DbContext context, string contextName, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning("Спроба {Attempt}/{MaxRetries} застосування міграцій для {ContextName} невдала: {Message}",
                    attempt, maxRetries, contextName, ex.Message);

                await Task.Delay(TimeSpan.FromSeconds(attempt * 2)); // Exponential backoff
            }
        }

        // Остання спроба без catch
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Перевіряє стан підключення до бази даних
    /// </summary>
    /// <typeparam name="TContext">Тип DbContext</typeparam>
    /// <returns>True якщо підключення успішне</returns>
    public async Task<bool> CheckDatabaseConnectionAsync<TContext>()
        where TContext : DbContext
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            _logger.LogInformation("Перевіряємо підключення до бази даних для {ContextName}...", typeof(TContext).Name);

            await context.Database.CanConnectAsync();

            _logger.LogInformation("Підключення до бази даних для {ContextName} успішне", typeof(TContext).Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка підключення до бази даних для {ContextName}", typeof(TContext).Name);
            return false;
        }
    }
}
