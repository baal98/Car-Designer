namespace AdvertisingAgency.Services.ProjectSharing
{
    public class SharedProjectViewModel
    {
        public Guid UserId { get; set; }
        public Guid CanvasId { get; set; }
        public string? Username { get; set; }
        public string? CanvasName { get; set; }
        public string? Description { get; set; }
        public string? Thumbnail { get; set; }
    }
}
