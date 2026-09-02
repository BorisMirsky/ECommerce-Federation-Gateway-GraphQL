using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Service.Users.Data;
using ECommerce.Service.Users.Models;

namespace ECommerce.Service.Users.Tests;

public class CartMutationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client = null!;
    private IServiceScope _scope = null!;
    private AppDbContext _db = null!;

    public CartMutationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Удаляем реальный DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Добавляем InMemory БД с УНИКАЛЬНЫМ именем для каждого теста
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                }, ServiceLifetime.Singleton); // <-- Важно!
            });
        });
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        // Получаем доступ к БД через сервисы
        _scope = _factory.Services.CreateScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Очищаем и создаём БД
        await _db.Database.EnsureDeletedAsync();
        await _db.Database.EnsureCreatedAsync();

        // Сид-данные
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