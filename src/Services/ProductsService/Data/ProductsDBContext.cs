using Microsoft.EntityFrameworkCore;
using ProductsService.Models;


namespace ProductsService.Data;

public class ProductsDBContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductEmbedding> ProductEmbeddings { get; set; } // Додаємо DbSet для ProductEmbedding

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Налаштування для Product
        modelBuilder.Entity<Product>()
            .HasKey(p => p.Id);

        // Налаштування для Category
        modelBuilder.Entity<Category>()
            .HasKey(c => c.Id);

        // Налаштування для ProductEmbedding
        modelBuilder.Entity<ProductEmbedding>()
            .HasKey(pe => pe.ProductId);
    }
}