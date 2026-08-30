using HotChocolate.Stitching;



var builder = WebApplication.CreateBuilder(args);

await Task.Delay(TimeSpan.FromSeconds(5));


var productsSdl = await new HttpClient().GetStringAsync("http://localhost:5001/graphql?sdl");
var usersSdl = await new HttpClient().GetStringAsync("http://localhost:5002/graphql?sdl");

builder.Services
    .AddGraphQLServer()
    .AddDocumentFromString(productsSdl)
    .AddDocumentFromString(usersSdl)
    .AddTypeExtension<object>();

var app = builder.Build();

app.MapGraphQL();
app.Urls.Add("http://localhost:5000");

app.Run();