namespace AdvertisingAgency.Common.DTOs
{
    public class SharedProjectDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? SharingUserId { get; set; }
        public ApplicationUserDTO? SharingUser { get; set; }

        public Guid? CollectingUserId { get; set; }

        public Guid? CanvasId { get; set; }

        public CanvasDto? Canvas { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
