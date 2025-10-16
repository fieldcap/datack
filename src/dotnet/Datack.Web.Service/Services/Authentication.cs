using Datack.Web.Data.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Datack.Web.Service.Services;

public class Authentication(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, UserRepository userRepository)
{
    public static Guid PasswordResetToken { get; } = Guid.NewGuid();

    public async Task<IdentityResult> Register(String userName, String password)
    {
        var user = new IdentityUser(userName);

        return await userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> ResetPassword(String resetToken, String password)
    {
        await Task.Delay(5000); // Prevent brute force attacks

        if (resetToken == PasswordResetToken.ToString())
        {
            var user = await userRepository.GetUser();
            var identityUser = await userManager.FindByIdAsync(user!.Id);
            var token = await userManager.GeneratePasswordResetTokenAsync(identityUser!);
            identityUser = await userManager.FindByIdAsync(user!.Id);
            var result = await userManager.ResetPasswordAsync(identityUser!, token, password);

            return result;
        }

        throw new("Password reset token is invalid");
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