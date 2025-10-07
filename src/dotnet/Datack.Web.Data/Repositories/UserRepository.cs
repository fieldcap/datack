using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Data.Repositories;

public class UserRepository(DataContext dataContext)
{
    public async Task<IdentityUser?> GetUser()
    {
        return await dataContext.Users.AsNoTracking().FirstOrDefaultAsync();
    }
}