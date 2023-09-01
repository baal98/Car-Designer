using AdvertisingAgency.Data.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AdvertisingAgency.Services.Interfaces;

namespace AdvertisingAgency.Web.Pages.Components
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly UserManager<ApplicationUser> _userManager;


        public CartCountViewComponent(IShoppingCartService shoppingCartService, UserManager<ApplicationUser> userManager)
        {
            _shoppingCartService = shoppingCartService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            var user = await _userManager.GetUserAsync(HttpContext.User as ClaimsPrincipal);

            var count = await _shoppingCartService.GetCartItemCountAsync(user);

            if (count == 0)
            {
                return Content(string.Empty);
            }

            return View(count);
        }
    }
}
