using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.AzureStorage;
using AdvertisingAgency.Services.Interfaces;

namespace AdvertisingAgency.Web.Areas.Identity.Pages.Account.Manage
{
    public class PersonImageModel : PageModel
    {
        private const long MaxFileSize = 2 * 1024 * 1024; // 5 MB

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAzureStorageService _azureBlobService;
        private readonly IImageService _imageService;

        public PersonImageModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAzureStorageService azureBlobService,
            IImageService imageService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _azureBlobService = azureBlobService;
            _imageService = imageService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Display(Name = "Profile Picture")]
            public IFormFile Image { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var imageUrl = user.ImageUrl;

            Input = new InputModel
            {
                Image = null
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

            if (Input.Image != null)
            {
                if (Input.Image.Length > MaxFileSize)
                {
                    ModelState.AddModelError("Input.Image", "The file is too large.");
                    return Page();
                }

                using var memoryStream = new MemoryStream();
                await Input.Image.CopyToAsync(memoryStream);
                var resizedImageBytes = _imageService.ResizeImage(memoryStream.ToArray(), 250);
                var blobUrl = await _azureBlobService.UploadFileBlobAsync(resizedImageBytes, Input.Image.FileName, Input.Image.ContentType);

                // If there was an existing image, delete it
                if (!string.IsNullOrEmpty(user.ImageUrl))
                {
                    await _azureBlobService.DeleteBlobDataAsync(user.ImageUrl);
                }

                user.ImageUrl = blobUrl;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set image.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile image has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // If there is an existing image, delete it
            if (!string.IsNullOrEmpty(user.ImageUrl))
            {
                await _azureBlobService.DeleteBlobDataAsync(user.ImageUrl);
                user.ImageUrl = null;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    StatusMessage = "Error: Unexpected issue occurred when trying to delete image.";
                    return RedirectToPage();
                }

                await _signInManager.RefreshSignInAsync(user);
                StatusMessage = "Error: Your profile image has been deleted.";
            }

            return RedirectToPage();
        }
    }
}
