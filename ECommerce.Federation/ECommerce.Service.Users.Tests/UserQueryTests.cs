using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Service.Users.Data;
using ECommerce.Service.Users.Models;

namespace ECommerce.Service.Users.Tests;

public class UserQueryTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly AppDbContext _db;
    private readonly IServiceScope _scope;

    public UserQueryTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptorsToRemove = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                                d.ServiceType == typeof(DbContextOptions) ||
                                d.ImplementationType == typeof(AppDbContext) ||
                                d.ServiceType == typeof(AppDbContext))
                    .ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite("Data Source=:memory:");
                });
            });
        });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _db.Database.EnsureCreated();

        _db.Users.AddRange(
            new User { Id = 1, Name = "Борис", Email = "boris@example.com" },
            new User { Id = 2, Name = "Анна", Email = "anna@example.com" }
        );

        _db.CartItems.AddRange(
            new CartItem { Id = 1, UserId = 1, ProductId = 1, Quantity = 2 },
            new CartItem { Id = 2, UserId = 1, ProductId = 3, Quantity = 1 },
            new CartItem { Id = 3, UserId = 2, ProductId = 2, Quantity = 1 }
        );

        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db?.Database.EnsureDeleted();
        _scope?.Dispose();
        _db?.Dispose();
        _client?.Dispose();
    }

    [Fact]
    public async Task GetUsers_ShouldReturnAllUsers()
    {
        var query = @"
            query {
                users {
                    id
                    name
                    email
                }
            }";

        var response = await _client.PostAsJsonAsync("/graphql", new { query });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        result.Should().Contain("Борис");
        result.Should().Contain("Анна");
        result.Should().Contain("boris@example.com");
    }

    [Fact]
    public async Task GetUser_ShouldReturnUserWithCart_WhenUserExists()
    {
        var query = @"
            query {
                user(id: 1) {
                    id
                    name
                    cartItems {
                        productId
                        quantity
                    }
                }
            }";

        var response = await _client.PostAsJsonAsync("/graphql", new { query });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        result.Should().Contain("Борис");
        result.Should().Contain("productId\":1");
        result.Should().Contain("productId\":3");
    }
}