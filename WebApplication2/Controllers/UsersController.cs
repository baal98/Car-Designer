using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Web.ViewModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.IO.Compression;
using System.Text;

namespace AdvertisingAgency.Web.Controllers
{
    /// <summary>
    /// Represents a controller for managing user-related actions in the application.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ICanvasMVCService _canvasService;
        private readonly IShoppingCartService _cartService;
        private readonly IOrderService _orderService;

        /// <summary>
        /// Initializes a new instance of the UsersController class with the specified dependencies.
        /// </summary>
        /// <param name="userManager">The user manager for managing user-related operations.</param>
        /// <param name="context">The database context.</param>
        /// <param name="canvasService">The service responsible for canvas-related operations.</param>
        /// <param name="cartService">The service responsible for shopping cart operations.</param>
        /// <param name="orderService">The service responsible for order-related operations.</param>
        public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ICanvasMVCService canvasService, IShoppingCartService cartService, IOrderService orderService)
        {
            _userManager = userManager;
            this._context = context;
            _canvasService = canvasService;
            _cartService = cartService;
            _orderService = orderService;
        }

        /// <summary>
        /// Promotes a user to admin role.
        /// </summary>
        /// <param name="id">The ID of the user to promote.</param>
        /// <returns>Redirects to the AllUsers action.</returns>
        [HttpPost("/users/promote")]
        public async Task<IActionResult> PromoteToAdmin([FromForm] string id)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.UserId.ToString() == id);
            if (user != null)
            {
                var result = await _userManager.AddToRoleAsync(user, "Admin");
                if (result.Succeeded)
                {
                    return RedirectToAction("AllUsers");
                }
            }
            return NotFound();
        }

        /// <summary>
        /// Demotes a user from admin role.
        /// </summary>
        /// <param name="id">The ID of the user to demote.</param>
        /// <returns>Redirects to the AllUsers action if successful, NotFound if not.</returns>
        [HttpPost("/users/demote")]
        public async Task<IActionResult> DemoteFromAdmin([FromForm] string id)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.UserId.ToString() == id);
            if (user != null)
            {
                var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
                if (result.Succeeded)
                {
                    return RedirectToAction("AllUsers");
                }
            }
            return NotFound();
        }

        /// <summary>
        /// Deletes a user from the application.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>Redirects to the AllUsers action if successful, BadRequest if not.</returns>
        [HttpPost("/users/delete")]
        public async Task<IActionResult> DeleteUser([FromForm] string id)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.UserId.ToString() == id);
            if (user != null)
            {
                var result = _context.ApplicationUsers.Remove(user);
                if (result != null)
                {
                    _context.SaveChanges();
                    return RedirectToAction("AllUsers");
                }
                else
                {
                    return BadRequest();
                }
            }
            return NotFound();
        }

        /// <summary>
        /// Displays a list of all users except the current user.
        /// </summary>
        /// <returns>View containing the list of users.</returns>
        public async Task<IActionResult> AllUsers()
        {
            var currentUserId = _userManager.GetUserId(User);
            var users = _userManager.Users.Where(u => u.Id != currentUserId).ToList();

            return View(users);
        }

        /// <summary>
        /// Retrieves and displays products associated with a specific user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <param name="page">The page number for pagination.</param>
        /// <returns>View containing the user's products.</returns>
        [HttpGet("/users/getuserproducts")]
        public async Task<IActionResult> GetUserProducts([FromQuery] string id, int page = 1)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.UserId.ToString() == id);
            if (user != null)
            {
                int projectsPerPage = 12;
                
                var projects = await _canvasService.GetCanvasProjects(user, page, projectsPerPage);

                var model = new UserProfileViewModel
                {
                    User = user,
                    Canvases = projects
                };

                int totalPages = await _canvasService.GetTotalPages(user, projectsPerPage);

                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = totalPages;

                return View(model);
            }
            return NotFound();
        }

        /// <summary>
        /// Retrieves and displays deleted products associated with a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="page">The page number for pagination.</param>
        /// <returns>View containing the user's deleted products.</returns>
        public async Task<IActionResult> GetUserProductsDeleted([FromQuery] string userId, int page = 1)
        {
            ApplicationUser? user = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.UserId.ToString() == userId);
            if (user != null)
            {
                int projectsPerPage = 12;

                var projects = await _canvasService.GetCanvasProjects(user, page, projectsPerPage);

                var model = new UserProfileViewModel
                {
                    User = user,
                    Canvases = projects
                };

                int totalPages = await _canvasService.GetTotalPages(user, projectsPerPage);

                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = totalPages;

                return View("GetUserProducts", model);

            }
            return NotFound();
        }

        /// <summary>
        /// Retrieves and displays detailed products associated with a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="page">The page number for pagination.</param>
        /// <returns>View containing the user's detailed products.</returns>
        [HttpGet("/users/getuserproductsdetails")]
        public async Task<IActionResult> GetUserProductsDetails([FromQuery] string userId, int page = 1)
        {
            var userIdentity = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == userId);

            if (userIdentity == null)
            {
                return NotFound();
            }
            if (userIdentity != null)
            {
                int projectsPerPage = 12;

                var projects = await _canvasService.GetCanvasProjects(userIdentity, page, projectsPerPage);

                var model = new UserProfileViewModel
                {
                    User = userIdentity,
                    Canvases = projects
                };

                return View("GetUserProducts", model);
            }
            return NotFound();
        }

        /// <summary>
        /// Displays details of a specific product.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="canvasId">The ID of the canvas (product).</param>
        /// <returns>View containing the product details.</returns>
        [HttpGet("/users/details")]
        public async Task<IActionResult> Details([FromQuery] string userId, Guid? canvasId)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == userId);

            var viewModel = await _canvasService.GetCanvasDetails(canvasId, userId);

            if (viewModel == null)
            {
                return RedirectToAction("Error", "Home");
            }
            ViewData["UserEmail"] = user.Email;
            return View(viewModel);
        }

        /// <summary>
        /// Removes a canvas (product) from the user's account.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="canvasId">The ID of the canvas (product) to remove.</param>
        /// <returns>Redirects to GetUserProductsDeleted action if successful, NotFound if not.</returns>
        [HttpPost("/users/remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string userId, Guid canvasId)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _canvasService.DeleteCanvas(canvasId, userId);

            if (!result)
            {
                return NotFound();
            }

            return RedirectToAction("GetUserProductsDeleted", new { userId = user.UserId.ToString(), page = 1 });
        }

        /// <summary>
        /// Retrieves user information.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>JSON containing user information.</returns>
        [HttpPost("/users/getuserinfo")]
        public async Task<IActionResult> GetUserInfo([FromForm] string id)
        {
            var user = await _context.ApplicationUsers
                .Include(u => u.City)
                .Include(u => u.Country)
                .Include(u => u.Address)
                .FirstOrDefaultAsync(x => x.UserId.ToString() == id);

            if (user != null)
            {
                return Json(new
                {
                    friendlyName = user.FriendlyName ?? "Not provided",
                    email = user.Email ?? "Not provided",
                    address = user.Address != null && user.City != null && user.Country != null
                        ? $"{user.Address.BuildingNumber}, {user.Address.Street} str., {user.City.Name}, {user.Country.Name}"
                        : "Not provided",
                    phoneNumber = user.PhoneNumber ?? "Not provided"
                });
            }

            return NotFound();
        }

        /// <summary>
        /// Displays a list of all users with their orders.
        /// </summary>
        /// <returns>View containing the list of users and their orders.</returns>
        [HttpGet("/users/allorders")]
        public async Task<IActionResult> AllOrders()
        {
            var model = await _cartService.GetAllUsersWithOrdersAsync();

            if (model == null || !model.Any(u => u.Orders.Count > 0))
            {
                TempData["ErrorMessage"] = "No orders found.";
                return RedirectToAction("AllUsers");
            }

            foreach (var userOrder in model)
            {
                foreach (var order in userOrder.Orders)
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        var canvasData = await _context.Canvases.FirstOrDefaultAsync(x => x.Id == detail.CanvasId);
                        string thumbnail = canvasData?.Thumbnail;
                        detail.Thumbnail = thumbnail;
                    }
                }
            }

            TempData["SuccessMessage"] = $"We found {model.Count} orders.";
            return View(model);
        }

        /// <summary>
        /// Displays a list of sorted orders within a specific date range.
        /// </summary>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>View containing the sorted orders.</returns>
        [HttpGet("/users/sortedorders")]
        public async Task<IActionResult> SortedOrders(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                endDate = endDate.Value.AddDays(1).AddTicks(-1);
            }

            var userOrders = await _cartService.GetUserSortedOrdersAsync(startDate, endDate);

            var model = new List<UserOrdersViewModel>();

            foreach (var uo in userOrders)
            {
                var orderViewModel = new OrderViewModel
                {
                    OrderId = uo.Orders.FirstOrDefault().OrderId,
                    UserId = uo.User.UserId.ToString(),
                    OrderDate = uo.Orders.FirstOrDefault().OrderDate,
                    OrderTotal = uo.Orders.Sum(order => order.OrderTotal),
                    OrderStatus = uo.Orders.FirstOrDefault()?.OrderStatus,
                    OrderDetails = new List<OrderDetailViewModel>()
                };

                foreach (var order in uo.Orders)
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        var canvasData = await _context.Canvases.FirstOrDefaultAsync(x => x.Id == detail.CanvasId);
                        string thumbnail = canvasData?.Thumbnail;

                        var orderDetailViewModel = new OrderDetailViewModel
                        {
                            ProductId = detail.ProductId,
                            ProductTitle = detail.ProductTitle,
                            CanvasId = detail.CanvasId,
                            Quantity = detail.Quantity,
                            Price = detail.Price,
                            Thumbnail = thumbnail
                        };

                        orderViewModel.OrderDetails.Add(orderDetailViewModel);
                    }
                }

                model.Add(new UserOrdersViewModel
                {
                    User = uo.User,
                    Orders = new List<OrderViewModel> { orderViewModel },
                    StartDate = startDate,
                    EndDate = endDate,
                    UserId = uo.User.UserId.ToString(),
                    CanvasId = orderViewModel.OrderDetails.FirstOrDefault().CanvasId,
                    FriendlyName = uo.User.FriendlyName,
                    Thumbnail = orderViewModel.OrderDetails.FirstOrDefault()?.Thumbnail,
                    OrderId = orderViewModel.OrderId,
                    OrderTotal = orderViewModel.OrderTotal,
                    OrderDate = orderViewModel.OrderDate,
                });
            }

            if (model.Count == 0)
            {
                TempData["ErrorMessage"] = "No orders found for this period.";
                return View(model);
            }

            TempData["SuccessMessage"] = $"We found {model.Count} orders.";
            return View(model);
        }

        /// <summary>
        /// Displays a list of orders for a specific user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>View containing the list of orders for the user.</returns>
        [HttpGet("/users/userorders")]
        public async Task<IActionResult> UserOrders(string id)
        {
            var user = await _context.ApplicationUsers
                .Include(x => x.Address)
                .Include(x => x.Country)
                .Include(x => x.City)
                .FirstOrDefaultAsync(x => x.UserId.ToString() == id);

            var userOrders = await _cartService.GetUserOrdersAsync(id);

            if (userOrders == null || userOrders.Orders.Count == 0)
            {
                TempData["ErrorMessage"] = "No orders found for this user.";
                return RedirectToAction("AllUsers");
            }

            if (user == null)
            {
                return RedirectToAction("AllUsers");
            }

            var model = new UserOrdersViewModel
            {
                User = user,
                Orders = new List<OrderViewModel>(),
            };

            foreach (var order in userOrders.Orders)
            {
                var orderViewModel = new OrderViewModel
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    OrderTotal = order.OrderTotal,
                    OrderStatus = order.OrderStatus,
                    OrderDetails = new List<OrderDetailViewModel>()
                };

                foreach (var detail in order.OrderDetails)
                {
                    var canvasData = await _context.Canvases.FirstOrDefaultAsync(x => x.Id == detail.CanvasId);
                    string thumbnail = canvasData?.Thumbnail;

                    var orderDetailViewModel = new OrderDetailViewModel
                    {
                        ProductId = detail.ProductId,
                        ProductTitle = detail.ProductTitle,
                        Quantity = detail.Quantity,
                        Price = detail.Price,
                        CanvasId = detail.CanvasId,
                        Thumbnail = thumbnail
                    };

                    orderViewModel.OrderDetails.Add(orderDetailViewModel);
                }

                model.Orders.Add(orderViewModel);
            }

            if (model.Orders.Count > 0)
            {
                model.CanvasId = model.Orders[0].OrderDetails[0].CanvasId;
                model.Thumbnail = model.Orders[0].OrderDetails[0].Thumbnail;
            }

            TempData["SuccessMessage"] = $"We found {model.Orders.Count} orders.";

            return View(model);
        }

        /// <summary>
        /// Displays a list of sorted orders for a specific user within a date range.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>View containing the sorted orders for the user.</returns>
        [HttpGet("/users/usersortedorders")]
        public async Task<IActionResult> UserSortedOrders(string id, DateTime? startDate, DateTime? endDate)
        {
            var user = await _context.ApplicationUsers
                .Include(x => x.Address)
                .Include(x => x.Country)
                .Include(x => x.City)
                .FirstOrDefaultAsync(x => x.UserId.ToString() == id);

            if (user == null)
            {
                return RedirectToAction("AllUsers");
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                endDate = endDate.Value.AddDays(1).AddTicks(-1);
            }

            var userOrders = await _cartService.GetUserSortedOrdersAsync(id, startDate, endDate);

            var model = new UserOrdersViewModel
            {
                User = user,
                Orders = userOrders.Orders
            };

            if (model.Orders.Count == 0)
            {
                TempData["ErrorMessage"] = "No orders found for this period.";
                return View("UserOrders", model);
            }
            TempData["SuccessMessage"] = $"We found {model.Orders.Count} orders.";
            return View("UserOrders", model);
        }

        /// <summary>
        /// Downloads order details for a user as a ZIP archive.
        /// </summary>
        /// <param name="Id">The ID of the user.</param>
        /// <param name="canvasId">The ID of the canvas related to the order.</param>
        /// <returns>The downloaded ZIP archive.</returns>
        public async Task<IActionResult> DownloadOrder(string Id, Guid canvasId)
        {
            var user = await _context.ApplicationUsers
                    .Include(x => x.Address)
                    .Include(x => x.Country)
                    .Include(x => x.City)
                    .FirstOrDefaultAsync(x => x.Id == Id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "File download failed! Please, try again!";
                return RedirectToAction("AllUsers");
            }

            if (string.IsNullOrWhiteSpace(Id))
            {
                return BadRequest();
            }

            try
            {
                var canvasData = await _orderService.GetOrderData(user, canvasId);

                if (canvasData == null || !canvasData.Any(x => x.type == "image" || x.type == "path"))
                {
                    TempData["ErrorMessage"] = "File download failed! There is no content in order!";
                    return BadRequest();
                }

                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var canvasObject in canvasData)
                        {
                            if (canvasObject.type == "image")
                            {
                                byte[] fileBytes;
                                if (canvasObject.src.StartsWith("http"))
                                {
                                    // Използваме HttpClient, за да свалим картинката
                                    using (var client = new HttpClient())
                                    {
                                        fileBytes = await client.GetByteArrayAsync(canvasObject.src);
                                    }
                                }
                                else
                                {
                                    // Изваждаме base64 данните от src
                                    var base64Data = canvasObject.src.Split(new[] { "base64," }, StringSplitOptions.None)[1];
                                    // Преобразуваме base64 данните в байтове
                                    fileBytes = Convert.FromBase64String(base64Data);
                                }

                                // Добавяме файл в архива
                                var zipArchiveEntry = archive.CreateEntry($"{canvasObject.name}.jpg", CompressionLevel.Fastest);
                                using (var zipStream = zipArchiveEntry.Open())
                                {
                                    await zipStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                                }
                            }
                            else if (canvasObject.type == "path")
                            {
                                // Вече имаме десериализиран път в Path свойството
                                var pathData = canvasObject.Path;

                                // Създаваме SVG path data string от десериализирания обект
                                var pathDataString = pathData.Select(command => string.Join(' ', command)).Aggregate((a, b) => a + " " + b);

                                // Тук вместо да създадем нов SVG path с Fabric.js (както е в JavaScript),
                                // ще преобразуваме SVG path data string в байтове и ще го запазим като SVG файл
                                var svgString = $"<svg xmlns=\"http://www.w3.org/2000/svg\"><path d=\"{pathDataString}\" fill=\"{canvasObject.fill}\" stroke=\"{canvasObject.stroke}\" stroke-width=\"{canvasObject.strokeWidth}\"/></svg>";
                                var svgBytes = Encoding.UTF8.GetBytes(svgString);

                                // Добавяме SVG файл в архива
                                var zipArchiveEntry = archive.CreateEntry($"{canvasObject.name}.svg", CompressionLevel.Fastest);
                                using (var zipStream = zipArchiveEntry.Open())
                                {
                                    await zipStream.WriteAsync(svgBytes, 0, svgBytes.Length);
                                }
                            }

                        }
                    }
                    TempData["SuccessMessage"] = $"Download complete with {canvasData.Count} products.";
                    // Връщаме ZIP архива като файл за изтегляне
                    return File(memoryStream.ToArray(), "application/zip", $"{user.UserId}_orders.zip");
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

    }

}
