using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Utility;
using AdvertisingAgency.Web.ViewModels.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvertisingAgency.Services.Interfaces
{
    public interface IShoppingCartService
    {
        Task<ShoppingCart> GetCartAsync(ApplicationUser user);

        Task<int> GetCartItemCountAsync(ApplicationUser user);

        Task AddToCartAsync(double productsPrice, string productDescription, int categoryId, ApplicationUser user,
            string userNameClaim, string url, string canvasName, Guid canvasId, string thumbnail);

        Task<IEnumerable<CartItem>> GetCartItemsAsync(ApplicationUser user);

        Task ClearCartAsync(ApplicationUser user);

        Task DecrementCartItemQuantityAsync(int cartItemId, ApplicationUser user);

        Task IncrementCartItemQuantityAsync(int cartItemId, ApplicationUser user);

        Task RemoveCartItemAsync(int cartItemId, ApplicationUser user);

        Task<(bool, string)> CheckUserProfileComplete(ApplicationUser user);

        Task<bool> CreateOrderAsync(ApplicationUser user, shoppingCartVM shoppingCartVM);

        Task<bool> RemoveCartAsync(ApplicationUser user);

        Task<List<UserOrdersViewModel>> GetAllUsersWithOrdersAsync();

        Task<List<UserOrdersViewModel>> GetUserSortedOrdersAsync(DateTime? startDate, DateTime? endDate);

        Task<List<UserOrdersViewModel>> GetUserOrders(string id);

        Task<UserOrdersViewModel> GetUserOrdersAsync(string id);

        Task<UserOrdersViewModel> GetUserSortedOrdersAsync(string id, DateTime? startDate, DateTime? endDate);
    }
}
