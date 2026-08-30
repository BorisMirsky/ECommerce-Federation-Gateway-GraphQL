using Microsoft.EntityFrameworkCore;
using ECommerce.Service.Users.Models;



namespace ECommerce.Service.Users.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "Борис", Email = "boris@example.com" },
            new User { Id = 2, Name = "Анна", Email = "anna@example.com" }
        );

        modelBuilder.Entity<CartItem>().HasData(
            new CartItem { Id = 1, UserId = 1, ProductId = 1, Quantity = 2 },
            new CartItem { Id = 2, UserId = 1, ProductId = 3, Quantity = 1 },
            new CartItem { Id = 3, UserId = 2, ProductId = 2, Quantity = 1 }
        );
    }
}