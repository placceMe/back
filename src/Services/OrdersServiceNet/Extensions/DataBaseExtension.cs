using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OrdersServiceNet.Extensions;

public static class DatabaseExtensions
{
    /// <summary>
    /// Застосовує міграції для вказаного DbContext при старті додатку
    /// </summary>
    /// <typeparam name="TContext">Тип DbContext</typeparam>
    /// <param name="serviceProvider">Service provider</param>
    /// <returns>Service provider для подальшого використання</returns>
    public static async Task<IServiceProvider> MigrateDatabaseAsync<TContext>(this IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetService<ILogger>();

        try
        {
            logger?.LogInformation("Перевіряємо підключення та міграції для {ContextName}...", typeof(TContext).Name);

            // Спочатку перевіряємо підключення до БД
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                logger?.LogError("Не вдається підключитися до бази даних для {ContextName}", typeof(TContext).Name);
                throw new InvalidOperationException($"Не вдається підключитися до бази даних для {typeof(TContext).Name}");
            }

            // Забезпечуємо створення бази даних (якщо не існує)
            var created = await context.Database.EnsureCreatedAsync();
            if (created)
            {
                logger?.LogInformation("База даних створена для {ContextName}", typeof(TContext).Name);
                return serviceProvider; // Якщо БД щойно створена, міграції не потрібні
            }

            // Перевіряємо чи потрібні міграції
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

            logger?.LogInformation("Для {ContextName}: застосовано {Applied} міграцій, очікується {Pending}",
                typeof(TContext).Name, appliedMigrations.Count(), pendingMigrations.Count());

            if (pendingMigrations.Any())
            {
                logger?.LogInformation("Застосовуємо {Count} міграцій для {ContextName}: {Migrations}",
                    pendingMigrations.Count(),
                    typeof(TContext).Name,
                    string.Join(", ", pendingMigrations));

                // Застосовуємо міграції з retry логікою
                await RetryMigrateAsync(context, logger, typeof(TContext).Name);

                logger?.LogInformation("Міграції для {ContextName} успішно застосовані!", typeof(TContext).Name);
            }
            else
            {
                logger?.LogInformation("Міграції для {ContextName} актуальні, пропускаємо.", typeof(TContext).Name);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Помилка при застосуванні міграцій для {ContextName}: {Message}", typeof(TContext).Name, ex.Message);

            // Якщо помилка пов'язана з тим, що таблиця вже існує, спробуємо продовжити
            if (ex.Message.Contains("already exists") || ex.Message.Contains("вже існує"))
            {
                logger?.LogWarning("Деякі об'єкти БД вже існують для {ContextName}, продовжуємо...", typeof(TContext).Name);
                return serviceProvider;
            }

            throw;
        }

        return serviceProvider;
    }

    /// <summary>
    /// Виконує міграції з retry логікою
    /// </summary>
    private static async Task RetryMigrateAsync(DbContext context, ILogger? logger, string contextName, int maxRetries = 3)
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
                logger?.LogWarning("Спроба {Attempt}/{MaxRetries} застосування міграцій для {ContextName} невдала: {Message}",
                    attempt, maxRetries, contextName, ex.Message);

                await Task.Delay(TimeSpan.FromSeconds(attempt * 2)); // Exponential backoff
            }
        }

        // Остання спроба без catch
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Застосовує міграції для вказаного типу DbContext
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="contextType">Тип DbContext</param>
    /// <returns>Service provider для подальшого використання</returns>
    public static async Task<IServiceProvider> MigrateDatabaseAsync(this IServiceProvider serviceProvider, Type contextType)
    {
        using var scope = serviceProvider.CreateScope();
        var context = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);
        var logger = scope.ServiceProvider.GetService<ILogger>();

        try
        {
            logger?.LogInformation("Перевіряємо міграції для {ContextName}...", contextType.Name);

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                logger?.LogInformation("Застосовуємо {Count} міграцій для {ContextName}: {Migrations}",
                    pendingMigrations.Count(),
                    contextType.Name,
                    string.Join(", ", pendingMigrations));

                await context.Database.MigrateAsync();

                logger?.LogInformation("Міграції для {ContextName} успішно застосовані!", contextType.Name);
            }
            else
            {
                logger?.LogInformation("Міграції для {ContextName} актуальні, пропускаємо.", contextType.Name);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Помилка при застосуванні міграцій для {ContextName}", contextType.Name);
            throw;
        }

        return serviceProvider;
    }

    /// <summary>
    /// Застосовує міграції для кількох DbContext одночасно
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="contextTypes">Масив типів DbContext</param>
    /// <returns>Service provider для подальшого використання</returns>
    public static async Task<IServiceProvider> MigrateAllDatabasesAsync(
        this IServiceProvider serviceProvider,
        params Type[] contextTypes)
    {
        var logger = serviceProvider.GetService<ILogger>();

        logger?.LogInformation("Початок застосування міграцій для {Count} контекстів", contextTypes.Length);

        foreach (var contextType in contextTypes)
        {
            await serviceProvider.MigrateDatabaseAsync(contextType);
        }

        logger?.LogInformation("Завершено застосування міграцій для всіх контекстів");

        return serviceProvider;
    }

    /// <summary>
    /// Перевіряє стан підключення до бази даних
    /// </summary>
    /// <typeparam name="TContext">Тип DbContext</typeparam>
    /// <param name="serviceProvider">Service provider</param>
    /// <returns>True якщо підключення успішне</returns>
    public static async Task<bool> CheckDatabaseConnectionAsync<TContext>(this IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetService<ILogger>();

        try
        {
            logger?.LogInformation("Перевіряємо підключення до бази даних для {ContextName}...", typeof(TContext).Name);

            await context.Database.CanConnectAsync();

            logger?.LogInformation("Підключення до бази даних для {ContextName} успішне", typeof(TContext).Name);
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Помилка підключення до бази даних для {ContextName}", typeof(TContext).Name);
            return false;
        }
    }
}