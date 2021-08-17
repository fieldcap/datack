using System;
using System.Threading.Tasks;
using Datack.Web.Service.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Service.Services
{
    public class Authentication
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly DataContext _dataContext;

        public Authentication(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, DataContext dataContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dataContext = dataContext;
        }

        public async Task<IdentityResult> Register(String userName, String password)
        {
            var user = new IdentityUser(userName);

            return await _userManager.CreateAsync(user, password);
        }

        public async Task<SignInResult> Login(String userName, String password, Boolean isPersistent)
        {
            return await _signInManager.PasswordSignInAsync(userName, password, isPersistent, false);
        }

        public async Task<IdentityUser> GetUser()
        {
            return await _dataContext.Users.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
