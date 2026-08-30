using Microsoft.EntityFrameworkCore;
using ECommerce.Service.Products.Data;
using ECommerce.Service.Products.GraphQL;
using ECommerce.Service.Products.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ProductsDb"));


builder.Services.AddScoped<ProductReferenceResolver>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddType<ProductType>()
    //.RegisterService<ProductReferenceResolver>()
    .AddApolloFederation(); 

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new Product { Id = 1, Name = "Ноутбук", Price = 999.99m, Stock = 10, Category = "Электроника" },
            new Product { Id = 2, Name = "Смартфон", Price = 699.99m, Stock = 25, Category = "Электроника" },
            new Product { Id = 3, Name = "Наушники", Price = 149.99m, Stock = 50, Category = "Аудио" }
        );
        db.SaveChanges();
    }
}

app.MapGraphQL();
app.Urls.Add("http://localhost:5001");

app.Run();