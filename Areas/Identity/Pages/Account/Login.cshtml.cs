using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using BookHaven.Models;
using System;

namespace BookHaven.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        public LoginModel(
            SignInManager<User> signInManager, 
            UserManager<User> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public String ReturnUrl { get; set; }

        [TempData]
        public String ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public String Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public String Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(String returnUrl = null)
        {
            if (!String.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(String.Empty, ErrorMessage);
            }
            returnUrl = returnUrl ?? Url.Content("~/");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ReturnUrl = returnUrl;
        }

        public async Task<ActionResult> OnPostAsync(String returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignIn(Input.Email, Input.Password, Input.RememberMe);
                if (result.Succeeded)
                {
                    var user = await userManager.FindByEmailAsync(Input.Email);
                    if (!user.EmailConfirmed)
                        return RedirectToPage("./NotConfirmedEmail");
                    await userManager.RememberLoginDate(user);
                    return LocalRedirect(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Invalid login attempt.");
                    return Page();
                }
            }
            return Page();
        }
    }
}
