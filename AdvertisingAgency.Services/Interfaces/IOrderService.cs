using AdvertisingAgency.Data.Data.Models;

namespace AdvertisingAgency.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ApplicationUser> GetUserInfo(string userId);
        Task<List<OrderHeader>> GetOrdersForUser(string userId);
        Task<List<OrderDetail>> GetItemsForOrder(int orderId);
        Task<List<OrderHeader>> GetOrdersByDate(ApplicationUser user, DateTime startDate, DateTime endDate);
        Task<List<CanvasObject>> GetOrderData(ApplicationUser user, Guid canvasId);
    }
}