using Microsoft.EntityFrameworkCore;
using ECommerce.Service.Products.Models;

namespace ECommerce.Service.Products.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Ноутбук", Price = 999.99m, Stock = 10, Category = "Электроника" },
            new Product { Id = 2, Name = "Смартфон", Price = 699.99m, Stock = 25, Category = "Электроника" },
            new Product { Id = 3, Name = "Наушники", Price = 149.99m, Stock = 50, Category = "Аудио" }
        );
    }
}