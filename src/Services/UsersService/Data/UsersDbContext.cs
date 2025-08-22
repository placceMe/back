using Microsoft.EntityFrameworkCore;
using UsersService.Models;

namespace UsersService.Data;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {

    }


    public DbSet<User> Users => Set<User>();
    public DbSet<RegistrationUser> RegistrationUsers => Set<RegistrationUser>();
    public DbSet<SalerInfo> SalerInfos => Set<SalerInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema for all entities
        modelBuilder.HasDefaultSchema("users_service");

        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure SalerInfo entity
        modelBuilder.Entity<SalerInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationship with User
            entity.HasOne(s => s.User)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configure Contacts as JSON
            entity.Property(e => e.Contacts)
                  .HasConversion(
                      contacts => System.Text.Json.JsonSerializer.Serialize(contacts, (System.Text.Json.JsonSerializerOptions?)null),
                      json => System.Text.Json.JsonSerializer.Deserialize<List<Contact>>(json, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Contact>())
                  .HasColumnType("jsonb");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Only configure if not already configured (for design-time scenarios)
        if (!optionsBuilder.IsConfigured)
        {
            // Default connection string for design-time
            optionsBuilder.UseNpgsql("Host=localhost;Database=marketplace_db;Username=postgres;Password=postgres;Search Path=users_service;",
                b => b.MigrationsHistoryTable("__EFMigrationsHistory", "users_service"));
        }

        base.OnConfiguring(optionsBuilder);
    }
}
