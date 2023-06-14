using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BookHaven.Models;
using BookHaven.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookHaven.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private IEmailService emailService;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailService emailService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public String ReturnUrl { get; set; }

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

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public String Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public String ConfirmPassword { get; set; }
        }

        public void OnGet(String returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<ActionResult> OnPostAsync(String returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new User {
                    UserName = Input.UserName,
                    Email = Input.Email,
                    RegistrationDate = DateTime.Now,
                    LastLoginDate = DateTime.Now
                };
                var result = await userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action(
                        "ConfirmEmail",
                        "Home",
                        new { userId = user.Id, code = code },
                        protocol: HttpContext.Request.Scheme);
                    await emailService.SendEmailAsync(Input.Email, "Confirm your account",
                        $"Go through the link to complete registration: <a href='{callbackUrl}'>confirmation link</a>");
                    return RedirectToPage("./NotConfirmedEmail");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(String.Empty, error.Description);
                    }
                    return Page();
                }
            }
            return Page();
        }
    }
}
