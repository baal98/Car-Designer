using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Utility;
using AdvertisingAgency.Web.ViewModels.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgency.Services
{
    /// <summary>
    /// Provides functionality for managing shopping carts and related operations.
    /// </summary>
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the ShoppingCartService class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public ShoppingCartService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the shopping cart associated with the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The shopping cart of the user, or null if not found.</returns>
        public async Task<ShoppingCart> GetCartAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.ApplicationUser == user);

			return cart;
        }

        /// <summary>
        /// Gets the total count of items in the user's cart.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The total item count in the user's cart.</returns>
        public async Task<int> GetCartItemCountAsync(ApplicationUser user)
        {
            var cart = await GetCartAsync(user);
            if (cart == null)
            {
                return 0;
            }
            return cart.Items.Sum(i => i.Quantity);
        }

        /// <summary>
        /// Adds a product to the user's cart.
        /// </summary>
        public async Task AddToCartAsync(double productsPrice, string productDescription, int categoryId, ApplicationUser user, string userNameClaim, string url, string canvasName, Guid canvasId, string thumbnail)
        {
            var categoryName = Enum.GetName(typeof(CategoryType), categoryId);

            var product = new Product
            {
                Author = userNameClaim,
                Description = productDescription,
                Price = productsPrice,
                ImageUrl = url ?? String.Empty,
                Category = categoryName,
                CategoryId = categoryId,
                Title = canvasName,
                CanvasId = canvasId
            };

            _context.Products.Add(product);

            var canvas = await _context.Canvases.FirstOrDefaultAsync(c => c.Id == canvasId);

            var cart = await GetCartAsync(user);

            // check if item already exists in the cart
            var existingCartItem = cart?.Items?.FirstOrDefault(i => i.Product.Title == product.Title && i.Thumbnail == thumbnail);

            if (existingCartItem != null)
            {
                // if item exists, increase the quantity
                existingCartItem.Quantity++;
            }
            else
            {
                // if item does not exist, create a new one
                var cartItem = new CartItem
                {
                    Product = product,
                    Thumbnail = thumbnail,
                    Quantity = 1,
                    CanvasId = canvasId
                };

                if (cart == null)
                {
                    cart = new ShoppingCart
                    {
                        ApplicationUser = user,
                        Items = new List<CartItem> { cartItem },
                        Price = productsPrice * cartItem.Quantity,
                    };

                    _context.ShoppingCarts.Add(cart);
                }
                else
                {
                    cart.Items.Add(cartItem);
                }
            }

            cart.Price += productsPrice;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the cart items for the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>A list of cart items.</returns>
        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(ApplicationUser user)
        {
            return await _context.CartItems
                .Include(i => i.Product)
                .Where(i => i.ShoppingCart.ApplicationUser == user)
                .ToListAsync();
        }

        /// <summary>
        /// Clears the cart for the specified user.
        /// </summary>
        /// <param name="user">The user whose cart will be cleared.</param>
        public async Task ClearCartAsync(ApplicationUser user)
        {
            var cart = await GetCartAsync(user);

            if (cart != null)
            {
                _context.ShoppingCarts.Remove(cart);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Decrements the quantity of a cart item.
        /// </summary>
        public async Task DecrementCartItemQuantityAsync(int cartItemId, ApplicationUser user)
        {
            var cart = await GetCartAsync(user);

            if (cart != null)
            {
                var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
                if (cartItem != null)
                {
                    if (cartItem.Quantity > 1)
                    {
                        cartItem.Quantity--;
                    }
                    else
                    {
                        cart.Items.Remove(cartItem);
                    }

                    await _context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Increments the quantity of a cart item.
        /// </summary>
        public async Task IncrementCartItemQuantityAsync(int cartItemId, ApplicationUser user)
        {
            var cart = await GetCartAsync(user);

            if (cart != null)
            {
                var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
                if (cartItem != null)
                {
                    cartItem.Quantity++;
                    await _context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Removes a specific cart item for the specified user.
        /// </summary>
        public async Task RemoveCartItemAsync(int cartItemId, ApplicationUser user)
        {
            var cart = await GetCartAsync(user);

            if (cart != null)
            {
                var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
                if (cartItem != null)
                {
                    cart.Items.Remove(cartItem);
                    await _context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Checks if the user's profile is complete.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <returns>A tuple indicating whether the profile is complete and a related message.</returns>
        public async Task<(bool, string)> CheckUserProfileComplete(ApplicationUser user)
        {
            var isProfileIncomplete = (!user.PhoneNumberConfirmed) || user.AddressId == null
                                                                   || user.CityId == null
                                                                   || user.CountryId == null
                                                                   || user.FriendlyName == null;


            if (isProfileIncomplete)
            {
                return (false, "Вашият профил не е пълен. Моля, актуализирайте профила си преди да направите поръчка.");
            }

            return (true, "Профилът е пълен.");
        }

        /// <summary>
        /// Creates an order for the specified user.
        /// </summary>
        public async Task<bool> CreateOrderAsync(ApplicationUser user, shoppingCartVM shoppingCartVM)
		{
            var cart = await GetCartAsync(user);

			var currentUserInfo = await _context.ApplicationUsers
				.Include(x => x.Address)
				.Include(x => x.Country)
				.Include(x => x.City)
				.Where(x => x.Id == user.Id)
				.FirstOrDefaultAsync();

			string phoneNumber = currentUserInfo.PhoneNumber;

			if (currentUserInfo is Microsoft.AspNetCore.Identity.IdentityUser<string> identityUser)
			{
				phoneNumber = identityUser.PhoneNumber;
			}


			if (cart == null)
			{
				return false;
			}

			shoppingCartVM.OrderHeader = new OrderHeader
			{
				ApplicationUser = user,
				OrderDate = DateTime.Now,
				OrderStatus = SD.StatusPending,
				PaymentStatus = SD.PaymentStatusPending,
				Name = currentUserInfo.FriendlyName,
				PhoneNumber = phoneNumber,
				Address = currentUserInfo.Address.Street,
				BuildingNumber = currentUserInfo.Address.BuildingNumber,
				Country = currentUserInfo.Country,
				City = currentUserInfo.City
			};

			foreach (var cartItem in cart.Items)
			{
				shoppingCartVM.OrderHeader.OrderTotal += cartItem.Product.Price * cartItem.Quantity;
				var orderDetail = new OrderDetail
				{
					ProductId = cartItem.Product.Id,
					Price = cartItem.Product.Price,
					Count = cartItem.Quantity
				};
				shoppingCartVM.OrderHeader.OrderDetails.Add(orderDetail);
			}

			_context.OrderHeaders.Add(shoppingCartVM.OrderHeader);
			var result = await _context.SaveChangesAsync();

			return true;
		}

        /// <summary>
        /// Removes the shopping cart for the specified user.
        /// </summary>
        public async Task<bool> RemoveCartAsync(ApplicationUser user)
        {
            var cart = await GetCartAsync(user);
            if (cart == null)
            {
                return false;
            }

            _context.ShoppingCarts.Remove(cart);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        /// <summary>
        /// Retrieves all users with their orders.
        /// </summary>
        /// <returns>A list of users with their respective orders.</returns>
        public async Task<List<UserOrdersViewModel>> GetAllUsersWithOrdersAsync()
        {
            var usersWithOrders = await _context.ApplicationUsers
                .Include(u => u.OrderHeaders)
                .ThenInclude(oh => oh.OrderDetails)
                .Where(u => u.OrderHeaders.Any())
                .Select(u => new UserOrdersViewModel
                {
                    UserId = u.UserId.ToString(),
                    FriendlyName = u.FriendlyName,
                    Orders = u.OrderHeaders
                        .Where(oh => oh.OrderDetails.Any())
                        .Select(oh => new OrderViewModel
                        {
                            OrderId = oh.Id,
                            OrderDate = oh.OrderDate,
                            OrderTotal = oh.OrderTotal,
                            OrderStatus = oh.OrderStatus,
                            OrderDetails = oh.OrderDetails.Select(od => new OrderDetailViewModel
                            {
                                ProductId = od.ProductId,
                                ProductTitle = od.Product.Title,
                                Quantity = od.Count,
                                Price = od.Price,
                                CanvasId = od.Product.CanvasId
                            }).ToList()
                        }).ToList()
                }).ToListAsync();

            return usersWithOrders;
        }

        /// <summary>
        /// Retrieves sorted orders for all users within a specified date range.
        /// </summary>
        public async Task<List<UserOrdersViewModel>> GetUserSortedOrdersAsync(DateTime? startDate, DateTime? endDate)
        {
            var usersWithOrders = await _context.ApplicationUsers
                .Include(x => x.Address)
                .Include(x => x.Addresses)
                .Include(x => x.Country)
                .Include(x => x.City)
                .Include(u => u.OrderHeaders)
                .ThenInclude(oh => oh.OrderDetails)
                .Where(u => u.OrderHeaders.Any(oh => (!startDate.HasValue || oh.OrderDate >= startDate.Value) &&
                                                     (!endDate.HasValue || oh.OrderDate <= endDate.Value)))
                .Select(u => new UserOrdersViewModel
                {
                    UserId = u.UserId.ToString(),
                    FriendlyName = u.FriendlyName,
                    User = u,
                    Orders = u.OrderHeaders
                        .Where(oh => oh.OrderDetails.Any() &&
                                     (!startDate.HasValue || oh.OrderDate >= startDate.Value) &&
                                     (!endDate.HasValue || oh.OrderDate <= endDate.Value))
                        .Select(oh => new OrderViewModel
                        {
                            OrderId = oh.Id,
                            OrderDate = oh.OrderDate,
                            OrderTotal = oh.OrderTotal,
                            OrderStatus = oh.OrderStatus,
                            OrderDetails = oh.OrderDetails.Select(od => new OrderDetailViewModel
                            {
                                ProductId = od.ProductId,
                                ProductTitle = od.Product.Title,
                                Quantity = od.Count,
                                Price = od.Price,
                                CanvasId = od.Product.CanvasId
                            }).ToList()
                        }).ToList()
                }).ToListAsync();

            return usersWithOrders;
        }

        /// <summary>
        /// Retrieves the orders for a specific user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A list of orders for the user.</returns>
        public async Task<List<UserOrdersViewModel>> GetUserOrders(string id)
        {
            var userWithOrders = await _context.ApplicationUsers
                .Include(u => u.OrderHeaders)
                .ThenInclude(oh => oh.OrderDetails)
                .Where(u => u.Id == id)
                .Select(u => new UserOrdersViewModel
                {
                    UserId = u.UserId.ToString(),
                    FriendlyName = u.FriendlyName,
                    Orders = u.OrderHeaders
                        .Select(oh => new OrderViewModel
                        {
                            OrderId = oh.Id,
                            OrderDate = oh.OrderDate,
                            OrderTotal = oh.OrderTotal,
                            OrderStatus = oh.OrderStatus,
                            OrderDetails = oh.OrderDetails.Select(od => new OrderDetailViewModel
                            {
                                ProductId = od.ProductId,
                                ProductTitle = od.Product.Title,
                                Quantity = od.Count,
                                Price = od.Price,
                                CanvasId = od.Product.CanvasId
                            }).ToList()
                        }).ToList()
                })
                .ToListAsync();

            return userWithOrders;
        }

        /// <summary>
        /// Retrieves the orders and their details for a specific user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>Details of the user's orders.</returns>
        public async Task<UserOrdersViewModel> GetUserOrdersAsync(string id)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.UserId.ToString() == id);

            if (user == null)
            {
                return null;
            }

            var userWithOrders = await _context.ApplicationUsers
                .Include(u => u.OrderHeaders)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (userWithOrders.OrderHeaders == null || userWithOrders.OrderHeaders.Count == 0)
            {
                return null;
            }

            var userWithOrdersAndDetails = await _context.ApplicationUsers
                .Include(u => u.OrderHeaders)
                .ThenInclude(oh => oh.OrderDetails)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (userWithOrdersAndDetails.OrderHeaders.Any(oh => oh.OrderDetails == null || oh.OrderDetails.Count == 0))
            {
                return null;
            }

            var userOrdersViewModel = await _context.ApplicationUsers
                .Include(x => x.Address)
                .Include(y => y.Canvases)
                .Include(x => x.Addresses)
                .Include(x => x.Country)
                .Include(x => x.City)
                .Include(u => u.OrderHeaders)
                .ThenInclude(oh => oh.OrderDetails)
                .Where(u => u.UserId == user.UserId)
                .Select(u => new UserOrdersViewModel
                {
                    UserId = u.UserId.ToString(),
                    FriendlyName = u.FriendlyName,
                    Orders = u.OrderHeaders
                        .Select(oh => new OrderViewModel
                        {
                            OrderId = oh.Id,
                            OrderDate = oh.OrderDate,
                            OrderTotal = oh.OrderTotal,
                            OrderStatus = oh.OrderStatus,
                            OrderDetails = oh.OrderDetails.Select(od => new OrderDetailViewModel
                            {
                                ProductId = od.ProductId,
                                ProductTitle = od.Product.Title,
                                Quantity = od.Count,
                                Price = od.Price,
                                CanvasId = od.Product.CanvasId,
                                Thumbnail = u.Canvases.Select(c => c.Thumbnail).FirstOrDefault()
                            }).ToList()
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            return userOrdersViewModel;
        }

        /// <summary>
        /// Retrieves sorted orders for a specific user within a specified date range.
        /// </summary>
        public async Task<UserOrdersViewModel> GetUserSortedOrdersAsync(string id, DateTime? startDate, DateTime? endDate)
        {
            var userOrdersViewModel = await GetUserOrdersAsync(id);

            if (userOrdersViewModel != null)
            {
                userOrdersViewModel.Orders = userOrdersViewModel.Orders
                    .Where(o => (!startDate.HasValue || o.OrderDate.Date >= startDate.Value.Date) && (!endDate.HasValue || o.OrderDate.Date <= endDate.Value.Date))
                    .ToList();
            }

            return userOrdersViewModel;
        }

    }
}
