namespace AdvertisingAgency.Web.ViewModels.ViewModels
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public string UserId {get; set; }
        public DateTime OrderDate { get; set; }
        public double OrderTotal { get; set; }
        public string OrderStatus { get; set; }
        public List<OrderDetailViewModel> OrderDetails { get; set; }
        public string Thumbnail { get; set; }
        public Guid CanvasId { get; set; }
    }
}
