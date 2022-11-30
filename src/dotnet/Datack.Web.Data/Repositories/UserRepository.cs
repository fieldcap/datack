using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Data.Repositories;

public class UserRepository
{
    private readonly DataContext _dataContext;

    public UserRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<IdentityUser?> GetUser()
    {
        return await _dataContext.Users.AsNoTracking().FirstOrDefaultAsync();
    }
}