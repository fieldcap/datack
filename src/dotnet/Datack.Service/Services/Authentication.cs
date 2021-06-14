using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Datack.Data.Data;

namespace Datack.Service.Services
{
    public class Authentication
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserData _userData;

        public Authentication(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, UserData userData)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userData = userData;
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
            return await _userData.GetUser();
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
