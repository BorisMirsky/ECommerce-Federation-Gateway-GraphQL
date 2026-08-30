using ECommerce.Service.Users.Models;
using ECommerce.Service.Users.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service.Users.GraphQL;

public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Field(u => u.Id).Type<NonNullType<IntType>>();
        descriptor.Field(u => u.Name).Type<NonNullType<StringType>>();
        descriptor.Field(u => u.Email).Type<NonNullType<StringType>>();

        // Поле для корзины (с загрузкой из БД)
        descriptor
            .Field(u => u.CartItems)
            .Resolve(async ctx =>
            {
                var db = ctx.Service<AppDbContext>();
                var user = ctx.Parent<User>();
                return await db.CartItems
                    .Where(c => c.UserId == user.Id)
                    .ToListAsync();
            })
            .Type<ListType<CartItemType>>();
    }
}