using ECommerce.Service.Users.Models;

namespace ECommerce.Service.Users.GraphQL;

public class AddItemToCartPayload
{
    public AddItemToCartPayload(User user, bool isNewItem)
    {
        User = user;
        IsNewItem = isNewItem;
    }

    public User User { get; set; }
    public bool IsNewItem { get; set; } // Вернем флаг, чтобы знать, добавили мы новый или обновили старый
}