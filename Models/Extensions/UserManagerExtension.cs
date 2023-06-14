using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.Security.Claims;

namespace BookHaven.Models
{
    public static class UserManagerExtension
    {
        public static async Task<IdentityResult> RememberLoginDate(this UserManager<User> userManager, User user)
        {
            user.LastLoginDate = DateTime.Now;
            return await userManager.UpdateAsync(user);
        }

        public static async Task<bool> IsAdminAsync(this UserManager<User> userManager, ClaimsPrincipal principal)
        {
            var user = await userManager.GetUserAsync(principal);
            return user.IsAdmin;
        }

        public static async Task<bool> IsCreatorAsync(this UserManager<User> userManager, ClaimsPrincipal principal)
        {
            var user = await userManager.GetUserAsync(principal);
            return user.IsCreator;
        }
    }
}
