using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Service.Users.Data;
using ECommerce.Service.Users.Models;

namespace ECommerce.Service.Users.Tests;

public class CartMutationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly AppDbContext _db;
    private readonly IServiceScope _scope;

    public CartMutationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Удаляем все регистрации DbContext
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

                // Используем SQLite InMemory для тестов
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
    public async Task AddItemToCart_ShouldAddNewItem_WhenProductNotInCart()
    {
        var mutation = @"
            mutation {
                addItemToCart(input: {
                    userId: 1
                    productId: 99
                    quantity: 3
                }) {
                    isNewItem
                    user {
                        id
                        cartItems {
                            productId
                            quantity
                        }
                    }
                }
            }";

        var response = await _client.PostAsJsonAsync("/graphql", new { query = mutation });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        result.Should().Contain("isNewItem\":true");
        result.Should().Contain("productId\":99");
        result.Should().Contain("quantity\":3");
    }

    [Fact]
    public async Task AddItemToCart_ShouldUpdateQuantity_WhenProductAlreadyInCart()
    {
        var mutation = @"
            mutation {
                addItemToCart(input: {
                    userId: 1
                    productId: 1
                    quantity: 5
                }) {
                    isNewItem
                    user {
                        id
                        cartItems {
                            productId
                            quantity
                        }
                    }
                }
            }";

        var response = await _client.PostAsJsonAsync("/graphql", new { query = mutation });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        result.Should().Contain("isNewItem\":false");
        result.Should().Contain("productId\":1");
        result.Should().Contain("quantity\":7");
    }

    [Fact]
    public async Task RemoveItemFromCart_ShouldRemoveItem_WhenExists()
    {
        var mutation = @"
            mutation {
                removeItemFromCart(userId: 1, productId: 3)
            }";

        var response = await _client.PostAsJsonAsync("/graphql", new { query = mutation });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        result.Should().Contain("removeItemFromCart\":true");
    }

    [Fact]
    public async Task RemoveItemFromCart_ShouldReturnFalse_WhenProductNotFound()
    {
        var mutation = @"
            mutation {
                removeItemFromCart(userId: 1, productId: 999)
            }";

        var response = await _client.PostAsJsonAsync("/graphql", new { query = mutation });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        result.Should().Contain("removeItemFromCart\":false");
    }

    [Fact]
    public async Task ClearCart_ShouldRemoveAllItems_WhenUserExists()
    {
        var mutation = @"
            mutation {
                clearCart(userId: 1)
            }";

        var response = await _client.PostAsJsonAsync("/graphql", new { query = mutation });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        result.Should().Contain("clearCart\":true");
    }

    [Fact]
    public async Task ClearCart_ShouldReturnFalse_WhenUserNotFound()
    {
        var mutation = @"
            mutation {
                clearCart(userId: 999)
            }";

        var response = await _client.PostAsJsonAsync("/graphql", new { query = mutation });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        result.Should().Contain("clearCart\":false");
    }
}