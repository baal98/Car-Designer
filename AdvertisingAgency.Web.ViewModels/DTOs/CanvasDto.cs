using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AdvertisingAgency.Web.ViewModels.DTOs
{
    public class CanvasDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? Name { get; set; }

        [JsonProperty("baseObject")]
        public BaseObjectDto? BaseObject { get; set; }

        [JsonProperty("objects")]
        public CanvasObjectDto[]? CanvasObjects { get; set; }

        [JsonProperty("thumbnail")]
        public string? Thumbnail { get; set; }

        [MaxLength(300)]
        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("createdOn")]
        public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
