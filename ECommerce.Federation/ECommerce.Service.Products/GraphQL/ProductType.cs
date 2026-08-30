using ECommerce.Service.Products.Models;

namespace ECommerce.Service.Products.GraphQL;

public class ProductType : ObjectType<Product>
{
    protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor.Field(p => p.Id).Type<NonNullType<IntType>>();
        descriptor.Field(p => p.Name).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.Price).Type<NonNullType<DecimalType>>();
        descriptor.Field(p => p.Stock).Type<IntType>();
        descriptor.Field(p => p.Category).Type<StringType>();
        //descriptor.Field(p => p.Id).Type<NonNullType<IntType>>().Key();
    }
}