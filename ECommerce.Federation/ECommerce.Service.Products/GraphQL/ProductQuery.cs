using Microsoft.EntityFrameworkCore;
using ECommerce.Service.Products.Data;
using ECommerce.Service.Products.Models;

namespace ECommerce.Service.Products.GraphQL;

public class ProductQuery
{
    public async Task<IEnumerable<Models.Product>> GetProductsAsync([Service] AppDbContext dbContext)
    {
        return await dbContext.Products.ToListAsync();
    }

    public async Task<Models.Product?> GetProductAsync(int id, [Service] AppDbContext dbContext)
    {
        return await dbContext.Products.FindAsync(id);
    }
}