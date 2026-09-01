using Microsoft.EntityFrameworkCore;
using ECommerce.Service.Users.Data;
using ECommerce.Service.Users.Models;

namespace ECommerce.Service.Users.GraphQL;

public class Mutation
{
    public async Task<AddItemToCartPayload> AddItemToCartAsync(
        AddItemToCartInput input,
        [Service] AppDbContext dbContext)
    {
        // 1. Ищем пользователя
        var user = await dbContext.Users
            .Include(u => u.CartItems)
            .FirstOrDefaultAsync(u => u.Id == input.UserId);

        if (user is null)
        {
            // В реальном проекте тут нужно кидать GraphQL ошибку, 
            // но для простоты вернем null (хотя лучше использовать ErrorType)
            throw new Exception($"User with id {input.UserId} not found.");
        }

        // 2. Ищем, есть ли уже этот товар в корзине
        var existingItem = user.CartItems
            .FirstOrDefault(c => c.ProductId == input.ProductId);

        bool isNewItem = false;

        if (existingItem is not null)
        {
            // Если есть - обновляем количество
            existingItem.Quantity += input.Quantity;
            isNewItem = false;
        }
        else
        {
            // Если нет - создаем новую запись
            var newItem = new CartItem
            {
                ProductId = input.ProductId,
                Quantity = input.Quantity,
                UserId = user.Id
            };
            user.CartItems.Add(newItem);
            isNewItem = true;
        }

        await dbContext.SaveChangesAsync();

        return new AddItemToCartPayload(user, isNewItem);
    }


    public async Task<bool> ClearCartAsync(int userId, [Service] AppDbContext dbContext)
    {
        var user = await dbContext.Users
            .Include(u => u.CartItems)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return false;

        user.CartItems.Clear();
        await dbContext.SaveChangesAsync();
        return true;
    }


    public async Task<bool> RemoveItemFromCartAsync(int userId, int productId, [Service] AppDbContext dbContext)
    {
        var user = await dbContext.Users
            .Include(u => u.CartItems)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return false;
        }

        var itemToRemove = user.CartItems
            .FirstOrDefault(c => c.ProductId == productId);

        if (itemToRemove is null)
        {
            return false; // Товара нет в корзине
        }

        user.CartItems.Remove(itemToRemove);
        await dbContext.SaveChangesAsync();
        return true;
    }
}