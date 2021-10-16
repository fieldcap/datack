using System;
using System.Threading.Tasks;
using Datack.Web.Data.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Datack.Web.Service.Services
{
    public class Authentication
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserRepository _userRepository;

        public Authentication(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, UserRepository userRepository)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userRepository = userRepository;
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
            return await _userRepository.GetUser();
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
