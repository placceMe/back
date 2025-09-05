using Microsoft.EntityFrameworkCore;
using ChatService.Models;

namespace ChatService.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("chat_service");

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ProductId, e.SellerId, e.BuyerId }).IsUnique();
                entity.HasIndex(e => e.SellerId);
                entity.HasIndex(e => e.BuyerId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                entity.HasMany(e => e.Messages)
                      .WithOne(m => m.Chat)
                      .HasForeignKey(m => m.ChatId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Body).IsRequired().HasMaxLength(4000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                entity.HasIndex(e => new { e.ChatId, e.CreatedAt });
            });
        }
    }
}
