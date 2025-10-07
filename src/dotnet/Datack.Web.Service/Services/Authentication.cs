using Datack.Web.Data.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Datack.Web.Service.Services;

public class Authentication(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, UserRepository userRepository)
{
    public async Task<IdentityResult> Register(String userName, String password)
    {
        var user = new IdentityUser(userName);

        return await userManager.CreateAsync(user, password);
    }

    public async Task<SignInResult> Login(String userName, String password, Boolean isPersistent)
    {
        return await signInManager.PasswordSignInAsync(userName, password, isPersistent, false);
    }

    public async Task<IdentityUser?> GetUser()
    {
        return await userRepository.GetUser();
    }

    public async Task Logout()
    {
        await signInManager.SignOutAsync();
    }
}