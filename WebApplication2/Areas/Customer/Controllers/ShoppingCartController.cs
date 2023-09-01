using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Web.Controllers;
using AdvertisingAgency.Web.ViewModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdvertisingAgency.Web.Areas.Customer.Controllers
{
    /// <summary>
    /// Represents a controller for managing shopping cart-related operations.
    /// </summary>
    [Area("Customer")]
	[Authorize]
	[Route("ShoppingCart")]
	public class ShoppingCartController : BaseController
	{
		private readonly IShoppingCartService _shoppingCartService;
		private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartController"/> class.
        /// </summary>
        /// <param name="shoppingCartService">The shopping cart service.</param>
        /// <param name="userManager">The user manager.</param>
        public ShoppingCartController(IShoppingCartService shoppingCartService, UserManager<ApplicationUser> userManager)
		{
			_shoppingCartService = shoppingCartService;
			_userManager = userManager;
		}

        /// <summary>
        /// Retrieves the currently authenticated user.
        /// </summary>
        /// <returns>The authenticated user.</returns>
        private async Task<ApplicationUser> GetAuthenticatedUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new Exception("User is not authenticated.");
            }
            return user;
        }

        /// <summary>
        /// Displays the shopping cart index view for the authenticated user.
        /// </summary>
        /// <returns>The view displaying the shopping cart items.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
		{
            ApplicationUser user;
            try
            {
                user = await GetAuthenticatedUser();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }

            user = await _userManager.GetUserAsync(User);

			if (user == null)
			{
				return RedirectToAction("Index", "Home");
			}

			var cart = await _shoppingCartService.GetCartAsync(user);
			var shoppingCartViewModel = new shoppingCartVM()
			{
				OrderHeader = new OrderHeader(),
			};
			if (cart != null && cart.Items != null)
			{
				shoppingCartViewModel.ShoppingCartItems = cart.Items;
				shoppingCartViewModel.ShoppingCartTotal = cart.Items.Sum(i => i.Product.Price * i.Quantity);
			}
			else
			{
				shoppingCartViewModel.ShoppingCartItems = new List<CartItem>();
				shoppingCartViewModel.ShoppingCartTotal = 0;
			}

			if (User.Identity.IsAuthenticated)
			{
				ViewBag.CurrentUsername = User.Identity.Name!;
			}

			return View(shoppingCartViewModel);
		}

        /// <summary>
        /// Handles the addition of an item to the shopping cart.
        /// </summary>
        /// <param name="totalPrice">The total price of the item.</param>
        /// <param name="description">The description of the item.</param>
        /// <param name="category">The category of the item.</param>
        /// <param name="url">The URL of the item.</param>
        /// <param name="canvasName">The name of the canvas.</param>
        /// <param name="canvasId">The ID of the canvas.</param>
        /// <param name="thumbnail">The thumbnail URL of the item.</param>
        /// <returns>Redirects to the shopping cart index view.</returns>
        [HttpPost("addToShoppingCart")]
		public async Task<IActionResult> AddToShoppingCart(double totalPrice, string description, int category, string url, string canvasName, Guid canvasId, string thumbnail)
		{
			var user = await _userManager.GetUserAsync(User);
			string userNameClaim = User.FindFirst(ClaimTypes.Name)?.Value;

			if (user == null)
			{
				return RedirectToAction("Index", "Home");
			}

			await _shoppingCartService.AddToCartAsync(totalPrice, description, category, user, userNameClaim, url, canvasName, canvasId, thumbnail);

			return RedirectToAction("Index");
		}

        /// <summary>
        /// Decrements the quantity of a cart item.
        /// </summary>
        /// <param name="cartItemId">The ID of the cart item to decrement.</param>
        /// <returns>Redirects to the shopping cart index view.</returns>
        [HttpPost("minus")]
		public async Task<IActionResult> Minus([FromForm] int cartItemId)
		{
			var user = await _userManager.GetUserAsync(User);

			if (user == null)
			{
				return RedirectToAction("Index", "Home");
			}

			await _shoppingCartService.DecrementCartItemQuantityAsync(cartItemId, user);

			return RedirectToAction("Index");
		}

        /// <summary>
        /// Increments the quantity of a cart item.
        /// </summary>
        /// <param name="cartItemId">The ID of the cart item to increment.</param>
        /// <returns>Redirects to the shopping cart index view.</returns>
        [HttpPost("plus")]
		public async Task<IActionResult> Plus([FromForm] int cartItemId)
		{
			var user = await _userManager.GetUserAsync(User);

			if (user == null)
			{
				return RedirectToAction("Index", "Home");
			}

			await _shoppingCartService.IncrementCartItemQuantityAsync(cartItemId, user);

			return RedirectToAction("Index");
		}

        /// <summary>
        /// Handles the removal of an item from the shopping cart.
        /// </summary>
        /// <param name="cartItemId">The ID of the cart item to remove.</param>
        /// <returns>Redirects to the shopping cart index view.</returns>
        [HttpPost("remove")]
		public async Task<IActionResult> Remove([FromForm] int cartItemId)
		{
			var user = await _userManager.GetUserAsync(User);

			if (user == null)
			{
				return RedirectToAction("Index", "Home");
			}

			await _shoppingCartService.RemoveCartItemAsync(cartItemId, user);

			return RedirectToAction("Index");
		}

        /// <summary>
        /// Handles the submission of the shopping cart for order creation.
        /// </summary>
        /// <param name="shoppingCartVM">The view model containing the shopping cart information.</param>
        /// <returns>The view for order summary or an error response.</returns>
        [HttpPost("summarypost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost([FromForm] shoppingCartVM shoppingCartVM)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var (isProfileComplete, message) = await _shoppingCartService.CheckUserProfileComplete(user);

            if (!isProfileComplete)
            {
                // Използваме sweet alert, за да покажем съобщение за грешка на потребителя
                TempData["ShoppingCartError"] = message;
                return RedirectToPage("/Account/Manage/Index", new { area = "Identity" });
            }

            await _shoppingCartService.CreateOrderAsync(user, shoppingCartVM);

            if (shoppingCartVM != null && shoppingCartVM.OrderHeader != null)
            {
                return View("Summary", shoppingCartVM);
            }

            return BadRequest();
        }

        /// <summary>
        /// Places the order by removing the cart and redirecting to the home page.
        /// </summary>
        /// <param name="shoppingCartVM">The view model containing the shopping cart information.</param>
        /// <returns>Redirects to the home page or a bad request response.</returns>
        [HttpPost("placeorder")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> PlaceOrder([FromForm] shoppingCartVM shoppingCartVM)
		{
			var user = await _userManager.GetUserAsync(User);

			if (user == null)
			{
				return RedirectToAction("Index", "Home");
			}

			var cartRemoved = await _shoppingCartService.RemoveCartAsync(user);

			if (cartRemoved)
			{
				return RedirectToAction("Index", "Home");
			}

			return BadRequest();
		}
	}

}
