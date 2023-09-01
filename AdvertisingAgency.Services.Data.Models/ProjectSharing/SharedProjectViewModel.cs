namespace AdvertisingAgency.Services.Data.Models.ProjectSharing
{
    public class SharedProjectViewModel
    {
        public Guid UserId { get; set; }
        public Guid CanvasId { get; set; }
        public string? Username { get; set; }
        public string? CanvasName { get; set; }
        public string? Description { get; set; }
        public string? Thumbnail { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
