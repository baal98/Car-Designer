namespace AdvertisingAgency.Web.ViewModels.ViewModels
{
    public class ProjectSearchViewModel
    {
        public Guid Id { get; set; }
        public AdvertisingAgency.Data.Data.Models.Canvas? Canvas { get; set; }
        public string? Name { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string? Thumbnail { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
