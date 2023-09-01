using AdvertisingAgency.Data.Data.Models;

namespace AdvertisingAgency.Web.ViewModels.ViewModels
{
    public class UserOrdersViewModel
    {
        public ApplicationUser User { get; set; }
        public string UserId { get; set; }
        public string FriendlyName { get; set; }
        public List<OrderViewModel> Orders { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid CanvasId { get; set; }
        public string? Thumbnail;

        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public double OrderTotal { get; set; }
        public string OrderStatus { get; set; }
    }
}
