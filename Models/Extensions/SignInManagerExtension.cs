using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookHaven.Models
{
    public static class SignInManagerExtension
    {
        public static Task<SignInResult> PasswordSignIn(
            this SignInManager<User> signInManager, 
            string email, 
            string password, 
            bool rememberMe, 
            bool lockoutOnFailure = false)
        {
            User user = signInManager.UserManager.FindByEmailAsync(email).Result;
            if (user == null)
            {
                return Task.FromResult(SignInResult.Failed);
            }
            if (user.IsBlocked)
            {
                return Task.FromResult(SignInResult.LockedOut);
            }
            return signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure);
        }
    }
}
