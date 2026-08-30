using ECommerce.Service.Users.Models;

namespace ECommerce.Service.Users.GraphQL;

public class CartItemType : ObjectType<CartItem>
{
    protected override void Configure(IObjectTypeDescriptor<CartItem> descriptor)
    {
        descriptor.Field(c => c.Id).Type<NonNullType<IntType>>();
        descriptor.Field(c => c.ProductId).Type<NonNullType<IntType>>();
        descriptor.Field(c => c.Quantity).Type<NonNullType<IntType>>();
        descriptor.Field(c => c.UserId).Type<NonNullType<IntType>>();
    }
}