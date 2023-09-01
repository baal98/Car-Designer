using AdvertisingAgency.Web.ViewModels.ViewModels;

namespace AdvertisingAgency.Services.Data.Models.Canvas
{
    public class CanvasViewModel
    {
        public Guid Id { get; set; }
        public AdvertisingAgency.Data.Data.Models.Canvas? Canvas { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Thumbnail { get; set; }
        public string? Description { get; set; }
        public List<CategoryViewModel> Categories { get; set; }
    }

}
