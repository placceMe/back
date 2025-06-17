using Microsoft.EntityFrameworkCore;
using UsersService.Models;

namespace UsersService.Data;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users_service"); // для UsersService
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users => Set<User>();
}
