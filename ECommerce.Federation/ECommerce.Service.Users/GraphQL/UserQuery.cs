using Microsoft.EntityFrameworkCore;
using ECommerce.Service.Users.Data;
using ECommerce.Service.Users.Models;

namespace ECommerce.Service.Users.GraphQL;

public class UserQuery
{
    public async Task<IEnumerable<User>> GetUsersAsync([Service] AppDbContext dbContext)
    {
        return await dbContext.Users.ToListAsync();
    }

    public async Task<User?> GetUserAsync(int id, [Service] AppDbContext dbContext)
    {
        return await dbContext.Users
            .Include(u => u.CartItems)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}