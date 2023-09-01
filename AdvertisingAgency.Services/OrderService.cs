using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgency.Services
{
    /// <summary>
    /// Provides services related to user orders.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves detailed information of a user based on their ID.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <returns>The details of the user.</returns>
        public async Task<ApplicationUser> GetUserInfo(string userId)
        {
            var user = await _context.ApplicationUsers
                .Include(x => x.Address)
                .Include(x => x.City)
                .Include(x => x.Country)
                .Where(oh => oh.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new Exception("The user was not found");
            }

            return user;
        }

        /// <summary>
        /// Retrieves all orders associated with a particular user.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <returns>A list of orders for the user.</returns>
        public async Task<List<OrderHeader>> GetOrdersForUser(string userId)
        {
            return await _context.OrderHeaders
                .Include(x => x.City)
                .Include(x => x.Country)
                .Where(oh => oh.ApplicationUser.Id == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Fetches all items associated with a particular order.
        /// </summary>
        /// <param name="orderId">The order's unique identifier.</param>
        /// <returns>A list of order details for the specified order.</returns>
        public async Task<List<OrderDetail>> GetItemsForOrder(int orderId)
        {
	        return await _context.OrderDetails
		        .Where(od => od.OrderId == orderId)
		        .Include(od => od.Product)
		        .ToListAsync();
        }

        /// <summary>
        /// Retrieves orders for a given user between specified start and end dates.
        /// </summary>
        /// <param name="user">The user object.</param>
        /// <param name="startDate">The start date for the range.</param>
        /// <param name="endDate">The end date for the range.</param>
        /// <returns>A list of orders within the date range for the user.</returns>
        public async Task<List<OrderHeader>> GetOrdersByDate(ApplicationUser user, DateTime startDate, DateTime endDate)
        {
            return await _context.OrderHeaders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.ApplicationUser == user && o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToListAsync(); ;
        }

        /// <summary>
        /// Fetches the data of a canvas based on the canvas ID for a given user.
        /// </summary>
        /// <param name="user">The user object.</param>
        /// <param name="canvasId">The canvas's unique identifier.</param>
        /// <returns>A list of canvas objects for the user and specified canvas ID.</returns>
        public async Task<List<CanvasObject>> GetOrderData(ApplicationUser user, Guid canvasId)
        {
            var canvas = await _context.Canvases
                .Include(o => o.Objects)
                .FirstOrDefaultAsync(o => o.Id == canvasId);

            if (canvas == null || canvas.Objects.Count == 0)
            {
                throw new Exception("Canvas not found");
            }

            return canvas.Objects;
        }
        
    }
}
