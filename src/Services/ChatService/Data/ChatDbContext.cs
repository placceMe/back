using Microsoft.EntityFrameworkCore;
using ChatService.Models;

namespace ChatService.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<Chat> Chats { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback configuration if needed
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Встановлюємо default схему
        modelBuilder.HasDefaultSchema("chat_service");

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SellerId, e.BuyerId, e.ProductId }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.LastMessageAt).HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Chat)
                  .WithMany(e => e.Messages)
                  .HasForeignKey(e => e.ChatId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.SentAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.HasIndex(e => e.ChatId);
            entity.HasIndex(e => e.SenderId);
        });
    }
}