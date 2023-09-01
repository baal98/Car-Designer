using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Data.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using static AdvertisingAgency.Data.Data.DataConstants;

namespace AdvertisingAgency.Web.Areas.Identity.Pages.Account.Manage
{

	public class AddressModel : PageModel
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _context;
		private readonly SignInManager<ApplicationUser> _signInManager;

		public AddressModel(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			ApplicationDbContext context)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_context = context;
		}

		[TempData]
		public string StatusMessage { get; set; }

		[BindProperty]
		public InputModel Input { get; set; }
		
		public class InputModel
		{
			[Required]
			[Display(Name = "Street")]
			[StringLength(AddressStreetMaxLength, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = AddressStreetMinLength)]
			public string Street { get; set; }

			[Required]
			[Display(Name = "Building Number")]
			[StringLength(AddressBuildingNumberMaxLength, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = AddressBuildingNumberMinLength)]
			public string BuildingNumber { get; set; }

			[Required]
			[Display(Name = "Additional Info")]
			[StringLength(AdditionalInfoMaxLength, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = AdditionalInfoMinLength)]
			public string AdditionalInfo { get; set; }

			[Required]
			[Display(Name = "Country")]
			[StringLength(CountryNameMaxLength, ErrorMessage = "The {0} must be at max {1} characters long.")]
			public string CountryName { get; set; }

			[Required]
			[Display(Name = "City")]
			[StringLength(CityMaxLength, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = CityMinLength)]
			public string CityName { get; set; }
		}

		private async Task LoadAsync(ApplicationUser user)
		{
			var address = await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == user.Id);
			var city = address != null ? await _context.Cities.FindAsync(address.CityId) : null;
			var country = city != null ? await _context.Countries.FindAsync(city.CountryId) : null;

			if (address == null && city == null && country == null)
			{
				// If no address, city or country is found, then the user has not set his address yet
				StatusMessage = "Please set your address.";
			}

			Input = new InputModel
			{
				Street = address?.Street,
				BuildingNumber = address?.BuildingNumber,
				AdditionalInfo = address?.AdditionalInfo,
				CityName = city?.Name,
				CountryName = country?.Name
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

			var address = await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == user.Id);
			var city = await _context.Cities.FirstOrDefaultAsync(c => c.Name.ToLower() == Input.CityName.ToLower());

			Country? country;
			if (!string.IsNullOrEmpty(Input.CountryName))
			{
				country = await _context.Countries.FirstOrDefaultAsync(c => c.Name.ToLower() == Input.CountryName.ToLower());
				if (country == null)
				{
					country = new Country
					{
						Name = Input.CountryName
					};
					_context.Countries.Add(country);
					await _context.SaveChangesAsync(); // Saving changes to generate ID for new country
				}
			}
			else
			{
				country = null; // Handle the case when no country is specified
			}

			if (city == null)
			{
				city = new City
				{
					Name = Input.CityName,
					CountryId = country?.Id ?? 0
				};
				_context.Cities.Add(city);
				await _context.SaveChangesAsync();
			}
			else if (country != null && city.CountryId != country.Id)
			{
				city.CountryId = country.Id;
				_context.Cities.Update(city);
				await _context.SaveChangesAsync();
			}

			if (address != null)
			{
				// Update existing address
				address.Street = Input.Street;
				address.BuildingNumber = Input.BuildingNumber;
				address.AdditionalInfo = Input.AdditionalInfo;
				address.CityId = city?.Id ?? 0;
				address.CountryId = country?.Id ?? 0;
				user.AddressId = address.Id; // Set the AddressId in ApplicationUser
				user.CityId = address.CityId; // Set the CityId in ApplicationUser
				user.CountryId = address.CountryId; // Set the CountryId in ApplicationUser
				await _context.SaveChangesAsync(); // Save changes to generate AddressId
			}
			else
			{
				// Create new address
				address = new Address
				{
					UserId = user.Id,
					Street = Input.Street,
					BuildingNumber = Input.BuildingNumber,
					AdditionalInfo = Input.AdditionalInfo,
					CityId = city?.Id ?? 0,
					CountryId = country?.Id ?? 0
				};
				_context.Addresses.Add(address);
				await _context.SaveChangesAsync(); // Save changes to generate AddressId
				user.AddressId = address.Id; // Set the AddressId in ApplicationUser
				user.CityId = address.CityId; // Set the CityId in ApplicationUser
				user.CountryId = address.CountryId; // Set the CountryId in ApplicationUser
				await _userManager.UpdateAsync(user); // Update the ApplicationUser with the AddressId, CityId, and CountryId
			}

			StatusMessage = "Your address has been updated";
			return RedirectToPage();
		}

		public async Task<IActionResult> OnPostDeleteAddressAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var address = await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == user.Id);
			if (address != null)
			{
				_context.Addresses.Remove(address);
				await _context.SaveChangesAsync();

				user.AddressId = null; // Clear the AddressId in ApplicationUser
				user.CityId = null; // Clear the CityId in ApplicationUser
				user.CountryId = null; // Clear the CountryId in ApplicationUser

				await _userManager.UpdateAsync(user); // Update the ApplicationUser
			}

			StatusMessage = "Your address has been deleted.";
			return RedirectToPage();
		}

	}
}
