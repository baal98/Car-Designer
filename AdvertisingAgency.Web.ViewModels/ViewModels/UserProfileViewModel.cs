using AdvertisingAgency.Data.Data.Models;

namespace AdvertisingAgency.Web.ViewModels.ViewModels
{
	public class UserProfileViewModel
	{
		public ApplicationUser User { get; set; }
		public List<OrderHeader> Orders { get; set; }
		public List<OrderDetail> OrderDetails { get; set; }
		public IQueryable<Canvas>? Canvases { get; set; }
	}
}
