using Microsoft.EntityFrameworkCore;
using ECommerce.Service.Products.Data;
using ECommerce.Service.Products.Models;
using HotChocolate.ApolloFederation.Resolvers;

namespace ECommerce.Service.Products.GraphQL;

public class ProductReferenceResolver
{
    [ReferenceResolver]
    public static async Task<Product?> GetProductAsync(
        [Map("id")] int id,
        [Service] AppDbContext dbContext)
    {
        return await dbContext.Products.FindAsync(id);
    }
}