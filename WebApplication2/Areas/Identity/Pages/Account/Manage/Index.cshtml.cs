using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AdvertisingAgency.Web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISmsService _smsService;
        private readonly IConfiguration _configuration;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ISmsService smsService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _smsService = smsService;
            _configuration = configuration;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public bool CodeSent { get; set; }
        public bool IsPhoneNumberConfirmed { get; set; } // Ново свойство за показване на статуса на потвърждението

        public class InputModel
        {
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Verification code")]
            public string Code { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };

            IsPhoneNumberConfirmed = await _userManager.IsPhoneNumberConfirmedAsync(user); // Инициализиране на статуса на потвърждението
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

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            var userWithSamePhone = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == Input.PhoneNumber);

            if (userWithSamePhone != null)
            {
                TempData["Error"] = "This phone number is already registered by another user. Please enter another phone number!";
                return RedirectToPage();
            }


            if (String.IsNullOrEmpty(Input.Code) && Input.PhoneNumber != phoneNumber)
            {
                var random = new Random();
                var verificationCode = random.Next(100000, 999999).ToString(); // Генериране на случаен код

                await Task.Delay(1000); // Забавяне от 15 секунди (15000 милисекунди)

                _smsService.SendSms(Input.PhoneNumber, $"Verification code: {verificationCode}");

                TempData["VerificationCode"] = verificationCode; // Запазване на кода за потвърждение в TempData

                CodeSent = true;
                await LoadAsync(user);
                return Page();
            }

            var savedVerificationCode = TempData["VerificationCode"] as string; // Извличане на запазения код за потвърждение от TempData

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.Code != savedVerificationCode) // Проверка на кода за потвърждение
            {
                ModelState.AddModelError(nameof(Input.Code), "Invalid verification code.");
                CodeSent = true;
                await LoadAsync(user);
                return Page();
            }

            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }

                user.PhoneNumberConfirmed = true; // Маркиране на PhoneNumberConfirmed като true
                await _userManager.UpdateAsync(user); // Запазване на промените в потребителя
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePhoneNumberAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, null);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to delete phone number.";
                return RedirectToPage();
            }

            user.PhoneNumberConfirmed = false;
            await _userManager.UpdateAsync(user);
            StatusMessage = "Your phone number has been deleted.";
            return RedirectToPage();
        }
    }
}
