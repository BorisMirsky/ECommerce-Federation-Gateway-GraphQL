namespace ECommerce.Service.Users.GraphQL;

public class AddItemToCartInput
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}