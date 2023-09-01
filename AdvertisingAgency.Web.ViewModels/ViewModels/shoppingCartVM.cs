using AdvertisingAgency.Data.Data.Models;

namespace AdvertisingAgency.Web.ViewModels.ViewModels
{
    public class shoppingCartVM
    {
        public IEnumerable<CartItem> ShoppingCartItems;
        public double ShoppingCartTotal;
        public OrderHeader? OrderHeader;
        public double Price;
        public string? Thumbnail;
        public Guid CanvasId;

        public List<CartItem>? ListCart { get; set; }
    }
}
