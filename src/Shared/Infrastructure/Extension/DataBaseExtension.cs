namespace OrdersServiceNet.Extensions;

public static class DataBaseExtension
{
    public static async Task<IServiceProvider> MigrateDatabaseAsync<TContext>(this IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                Console.WriteLine($"Applying {pendingMigrations.Count()} migrations for {typeof(TContext).Name}...");
                await context.Database.MigrateAsync();
                Console.WriteLine($"Migrations for {typeof(TContext).Name} applied successfully!");
            }
            else
            {
                Console.WriteLine($"Migrations for {typeof(TContext).Name} are up to date, skipping.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying migrations for {typeof(TContext).Name}: {ex.Message}");
            throw;
        }

        return serviceProvider;
    }

    public static async Task<IServiceProvider> MigrateDatabaseAsync(this IServiceProvider serviceProvider, Type contextType)
    {
        using var scope = serviceProvider.CreateScope();
        var context = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);

        try
        {
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                Console.WriteLine($"Applying {pendingMigrations.Count()} migrations for {contextType.Name}...");
                await context.Database.MigrateAsync();
                Console.WriteLine($"Migrations for {contextType.Name} applied successfully!");
            }
            else
            {
                Console.WriteLine($"Migrations for {contextType.Name} are up to date, skipping.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying migrations for {contextType.Name}: {ex.Message}");
            throw;
        }

        return serviceProvider;
    }
}
