using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Web.ViewModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisingAgency.Web.Controllers
{
    /// <summary>
    /// Represents a controller for managing home-related operations.
    /// </summary>
    public class HomeController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderService _orderService;

        /// <summary>
        /// Initializes a new instance of the HomeController class with the specified user manager and order service.
        /// </summary>
        /// <param name="userManager">The user manager for managing user-related operations.</param>
        /// <param name="orderService">The service responsible for order-related operations.</param>
        public HomeController(UserManager<ApplicationUser> userManager, IOrderService orderService)
        {
            this._userManager = userManager;
            _orderService = orderService;
        }

        /// <summary>
        /// Redirects to the CanvasMvc index action.
        /// </summary>
        /// <returns>A redirection to the CanvasMvc index action.</returns>
        public IActionResult Index()
        {
            return RedirectToAction("Index", "CanvasMvc");
        }

        /// <summary>
        /// Displays the user dashboard view.
        /// </summary>
        /// <returns>The view containing user dashboard information.</returns>
        [Authorize]
        public async Task<IActionResult> UserDashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userInfo = await _orderService.GetUserInfo(user.Id);

            if (userInfo == null)
            {
                userInfo = user;
            }

            var model = new UserProfileViewModel
            {
                User = userInfo,
                Orders = new List<OrderHeader>(),
                OrderDetails = new List<OrderDetail>()
            };

            return View(model);
        }

        /// <summary>
        /// Retrieves all orders for the current user and displays them.
        /// </summary>
        /// <returns>The view containing all orders for the current user.</returns>
        [Authorize]
        public async Task<IActionResult> GetAllOrders()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userInfo = await _orderService.GetUserInfo(user.Id);

            if (userInfo == null)
            {
                userInfo = user;
            }

            var orders = await _orderService.GetOrdersForUser(user.Id);

            var items = new List<OrderDetail>();

            foreach (var order in orders)
            {
                var orderItems = await _orderService.GetItemsForOrder(order.Id);
                items.AddRange(orderItems);
            }

            var model = new UserProfileViewModel
            {
                User = userInfo,
                Orders = orders,
                OrderDetails = items
            };

            return View(model);
        }

        /// <summary>
        /// Filters orders by date and displays the user dashboard view.
        /// </summary>
        /// <param name="startDate">The start date for filtering orders.</param>
        /// <param name="endDate">The end date for filtering orders.</param>
        /// <returns>The view containing filtered orders and user dashboard information.</returns>
        [HttpGet]
        public async Task<IActionResult> FilterOrders(DateTime? startDate, DateTime? endDate)
        {
            var user = await _userManager.GetUserAsync(User);

            var userInfo = await _orderService.GetUserInfo(user.Id);

            if (userInfo == null)
            {
                userInfo = user;
            }

            List<OrderHeader> orders = new List<OrderHeader>();

            if (startDate.HasValue && endDate.HasValue)
            {
                endDate = endDate.Value.AddDays(1).AddTicks(-1);
                orders = await _orderService.GetOrdersByDate(user, startDate.Value, endDate.Value);
            }

            var userProfileViewModel = new UserProfileViewModel
            {
                Orders = orders,
                OrderDetails = orders.SelectMany(o => o.OrderDetails).ToList(),
                User = user
            };

            return View("UserDashboard", userProfileViewModel);
        }

        /// <summary>
        /// Displays the user guide view.
        /// </summary>
        /// <returns>The user guide view.</returns>
        [AllowAnonymous]
        public IActionResult UserGuide()
        {
            return View();
        }

        /// <summary>
        /// Displays the contact us view.
        /// </summary>
        /// <returns>The contact us view.</returns>
        [AllowAnonymous]
        public IActionResult ContactUs()
        {
            return View();
        }

        /// <summary>
        /// Handles errors by displaying an error view with the corresponding status message.
        /// </summary>
        /// <param name="statusCode">The HTTP status code of the error.</param>
        /// <returns>The error view with the appropriate error message.</returns>
        [Route("Home/Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            string message = statusCode switch
            {
                404 => "Sorry, we couldn't find the page you were looking for.",
                500 => "Sorry, something went wrong on our side. We are looking into it.",
                _ => "An unexpected error occurred."
            };

            HttpError error = new HttpError(statusCode, message);
            return View("Error", error);
        }
    }
}
