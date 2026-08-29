using ECommerce.Service.Users.GraphQL;




var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>();

var app = builder.Build();

app.MapGraphQL();
app.Urls.Add("http://localhost:5002");

app.Run();