namespace AdvertisingAgency.Web.ViewModels.ViewModels
{
    public class OrderDetailViewModel
    {
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public Guid CanvasId { get; set; }
        public string Thumbnail { get; set; }
    }
}
