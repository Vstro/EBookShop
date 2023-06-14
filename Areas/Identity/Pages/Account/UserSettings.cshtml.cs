using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BookHaven.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookHaven.Areas.Identity.Pages.Account
{
    public class UserSettingsModel : PageModel
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        public UserSettingsModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [BindProperty(SupportsGet = true)]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [Display(Name = "Name")]
            public String UserName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public String Email { get; set; }

            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [DataType(DataType.Password)]
            [Display(Name = "Old Password")]
            public String OldPassword { get; set; }

            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public String NewPassword { get; set; }

            public bool IsValidating { get; set; }
        }

        public async Task<ActionResult> OnGet()
        {
            Input.IsValidating = false;
            User currentUser = await userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                if (currentUser.IsBlocked)
                {
                    return RedirectToPage("./Lockout");
                }
                if (!currentUser.EmailConfirmed)
                {
                    return RedirectToPage("./NotConfirmedEmail");
                }
                Input.UserName = currentUser.UserName;
                Input.Email = currentUser.Email;
            }
            return Page();
        }

        public async Task<ActionResult> OnPostAsync()
        {
            Input.IsValidating = true;
            if (ModelState.IsValid)
            {
                User currentUser = await userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    currentUser.UserName = Input.UserName;
                    currentUser.Email = Input.Email;
                }
                var result = await userManager.UpdateAsync(currentUser);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(String.Empty, error.Description);
                    }
                    return Page();
                }
                if (Input.OldPassword != null && Input.NewPassword != null)
                {
                    result = await userManager.ChangePasswordAsync(currentUser, Input.OldPassword, Input.NewPassword);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(String.Empty, error.Description);
                        }
                        return Page();
                    }
                }
            }
            return Page();
        }
    }
}