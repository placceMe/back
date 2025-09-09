using Microsoft.EntityFrameworkCore;
using ContentService.Models;

namespace ContentService.Data;

public class ContentDbContext : DbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
    {
    }

    public DbSet<Faq> Faqs { get; set; }
    public DbSet<MainBanner> MainBanners { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure schema for content service
        modelBuilder.HasDefaultSchema("content_service");

        // Configure Faq entity
        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Order);
            entity.HasIndex(e => e.IsVisible);

            entity.Property(e => e.Question).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).IsRequired().HasMaxLength(100);
        });

        // Configure MainBanner entity
        modelBuilder.Entity<MainBanner>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Order);
            entity.HasIndex(e => e.IsVisible);

            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).IsRequired().HasMaxLength(100);
        });
    }
}
