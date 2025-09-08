using Microsoft.EntityFrameworkCore;
using ProductsService.Models;


namespace ProductsService.Data;

public class ProductsDBContext : DbContext
{

    public ProductsDBContext(DbContextOptions<ProductsDBContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    // public DbSet<ProductEmbedding> ProductEmbeddings { get; set; }
    public DbSet<Characteristic> Characteristics { get; set; }
    public DbSet<CharacteristicDict> CharacteristicDicts { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema for all entities
        modelBuilder.HasDefaultSchema("products_service");

        modelBuilder.Entity<Product>()
             .HasKey(p => p.Id);
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category);
        modelBuilder.Entity<Product>()
            .HasMany(p => p.Characteristics)
            .WithOne()
            .HasForeignKey(c => c.ProductId);
        // Characteristic
        modelBuilder.Entity<Characteristic>()
            .HasKey(c => c.Id);

        // CharacteristicDict
        modelBuilder.Entity<CharacteristicDict>()
            .HasKey(cd => cd.Id);

        modelBuilder.Entity<Product>()
            .HasMany(p => p.Attachments)
            .WithOne(a => a.Product)
            .HasForeignKey(a => a.ProductId);

        // Category
        modelBuilder.Entity<Category>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Products)
            .WithOne(p => p.Category)
             .HasForeignKey(p => p.CategoryId);

        // // ProductEmbedding
        // modelBuilder.Entity<ProductEmbedding>()
        //     .HasKey(pe => pe.ProductId);

        // Attachment
        modelBuilder.Entity<Attachment>()
            .HasKey(a => a.Id);

        // Feedback
        modelBuilder.Entity<Feedback>()
            .HasKey(f => f.Id);
        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.Product)
            .WithMany()
            .HasForeignKey(f => f.ProductId);

        // // Rating
        // modelBuilder.Entity<Rating>()
        //     .HasKey(r => r.Id);

    }
}
