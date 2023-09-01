// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AdvertisingAgency.Data.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AdvertisingAgency.Data;
using AdvertisingAgency.Data.Data;

namespace AdvertisingAgency.Web.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;
        private readonly ApplicationDbContext _context;

        public DownloadPersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<DownloadPersonalDataModel> logger,
            ApplicationDbContext contex)
        {
            _userManager = userManager;
            _logger = logger;
            _context = contex;
        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

            var personalData = new Dictionary<string, string>();

            // Include user's personal data
            var personalDataProps = typeof(ApplicationUser).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var prop in personalDataProps)
            {
                personalData.Add(prop.Name, prop.GetValue(user)?.ToString() ?? "null");
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            // Include user's phone number
            personalData.Add("PhoneNumber", phoneNumber ?? "Phone number not provided");


            // Include user's friendly name
            personalData.Add("FriendlyName", user.FriendlyName);

            // Include user's image URL
            //personalData.Add("ImageUrl", user.ImageUrl);


            // Include user's image file name
            if (!string.IsNullOrEmpty(user.ImageUrl))
            {
                var imageUrl = new Uri(user.ImageUrl);
                personalData.Add("ImageFileName", Path.GetFileName(imageUrl.LocalPath));
            }

            // Include user's address, city, and country
            if (user.AddressId != null)
            {
                var address = await _context.Addresses
                    .Include(a => a.City)
                    .ThenInclude(c => c.Country)
                    .FirstOrDefaultAsync(a => a.Id == user.AddressId);

                if (address != null)
                {
                    personalData.Add("Street", address.Street);
                    personalData.Add("BuildingNumber", address.BuildingNumber);
                    personalData.Add("AdditionalInfo", address.AdditionalInfo);

                    personalData.Add("City", address.City?.Name ?? "null");
                    personalData.Add("Country", address.City?.Country?.Name ?? "null");
                }
            }

            // Include user's logins
            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var login in logins)
            {
                personalData.Add($"{login.LoginProvider} external login provider key", login.ProviderKey);
            }

            // Include user's authenticator key
            personalData.Add("Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));

            // Prepare the JSON file for download
            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData, options), "application/json");
        }



        //public async Task<IActionResult> OnPostAsync()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        //    }

        //    _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

        //    // Only include personal data for download
        //    var personalData = new Dictionary<string, string>();
        //    var personalDataProps = typeof(ApplicationUser).GetProperties().Where(
        //                    prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
        //    foreach (var p in personalDataProps)
        //    {
        //        personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
        //    }

        //    var logins = await _userManager.GetLoginsAsync(user);
        //    foreach (var l in logins)
        //    {
        //        personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
        //    }

        //    personalData.Add($"Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));

        //    Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
        //    return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
        //}
    }
}
