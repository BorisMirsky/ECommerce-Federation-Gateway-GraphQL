using Microsoft.EntityFrameworkCore;
using ECommerce.Service.Users.Data;
using ECommerce.Service.Users.GraphQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("UsersDb"));

builder.Services
    .AddGraphQLServer()
    .AddQueryType<UserQuery>()
    .AddMutationType<Mutation>()
    .AddType<UserType>()
    .AddType<CartItemType>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); 
}

app.MapGraphQL();
app.Urls.Add("http://localhost:5002");

app.Run();