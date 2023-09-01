using AdvertisingAgency.Data.Data.Models;

namespace AdvertisingAgency.Services.CanvasMVC
{
    // Създаваме този допълнителен клас, който има за пропърtи Canvas и TotalPrice, чрез които ще подаваме данните към View-то Details.
    public class CanvasViewModel
    {
        public Guid Id { get; set; }
        public Data.Data.Models.Canvas? Canvas { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Thumbnail { get; set; }
        public string? Description { get; set; }
    }
}
