namespace ECommerce.Service.Products.Models;
using HotChocolate.ApolloFederation;
using HotChocolate.ApolloFederation.Types;



public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
}