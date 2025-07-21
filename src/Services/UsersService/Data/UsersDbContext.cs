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
}
