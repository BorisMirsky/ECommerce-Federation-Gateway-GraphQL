using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Service.Users.Data;
using ECommerce.Service.Users.Models;

namespace ECommerce.Service.Users.Tests;

public class UserQueryTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client = null!;
    private IServiceScope _scope = null!;
    private AppDbContext _db = null!;

    public UserQueryTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                }, ServiceLifetime.Singleton);
            });
        });
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await _db.Database.EnsureDeletedAsync();
        await _db.Database.EnsureCreatedAsync();

        _db.Users.AddRange(
            new User { Id = 1, Name = "Борис", Email = "boris@example.com" },
            new User { Id = 2, Name = "Анна", Email = "anna@example.com" }
        );

        _db.CartItems.AddRange(
            new CartItem { Id = 1, UserId = 1, ProductId = 1, Quantity = 2 },
            new CartItem { Id = 2, UserId = 1, ProductId = 3, Quantity = 1 },
            new CartItem { Id = 3, UserId = 2, ProductId = 2, Quantity = 1 }
        );

        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _db.DisposeAsync();
    }

    private HttpClient CreateClient() => _client;

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