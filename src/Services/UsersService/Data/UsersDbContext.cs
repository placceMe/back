using Microsoft.EntityFrameworkCore;
using UsersService.Models;

namespace UsersService.Data;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
        //Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS users_service;");

    }
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     modelBuilder.HasDefaultSchema("users_service"); // для UsersService
    //     base.OnModelCreating(modelBuilder);
    // }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Налаштовуємо окрему таблицю міграцій для цього сервісу
        optionsBuilder.UseNpgsql(b => b.MigrationsHistoryTable("__EFMigrationsHistory", "users_service"));

        base.OnConfiguring(optionsBuilder);
    }
}
