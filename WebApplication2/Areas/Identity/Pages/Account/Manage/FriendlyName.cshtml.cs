using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AdvertisingAgency.Data.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvertisingAgency.Web.Areas.Identity.Pages.Account.Manage
{
    public class FriendlyNameModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public FriendlyNameModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Full Name")]
            [MaxLength(30)]
            public string? FriendlyName { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var friendlyName = user.FriendlyName;

            Username = userName;

            Input = new InputModel
            {
                FriendlyName = friendlyName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var friendlyName = user.FriendlyName;

            if (Input.FriendlyName != friendlyName)
            {
                user.FriendlyName = Input.FriendlyName;
                var updateUserResult = await _userManager.UpdateAsync(user);

                if (!updateUserResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set friendly name.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
