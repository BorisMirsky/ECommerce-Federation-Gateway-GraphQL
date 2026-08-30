using Microsoft.EntityFrameworkCore;
using ECommerce.Service.Products.Data;
using ECommerce.Service.Products.Models;

namespace ECommerce.Service.Products.GraphQL;

public class Query
{
    public async Task<IEnumerable<Product>> GetProductsAsync([Service] AppDbContext dbContext)
    {
        return await dbContext.Products.ToListAsync();
    }

    public async Task<Product?> GetProductAsync(int id, [Service] AppDbContext dbContext)
    {
        return await dbContext.Products.FindAsync(id);
    }
}