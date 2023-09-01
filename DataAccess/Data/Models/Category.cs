using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdvertisingAgency.Data.Data.Models
{
    public class Category
    {
        public int Id { get; set; }
        public CategoryType CategoryTypeName { get; set; }

        [Required]
        public string Name { get; set; }

        [DisplayName("Display order")]
        [Range(1, 100, ErrorMessage = "Display Order must be between 1 and 100 only!!")]
        public int DisplayOrder { get; set; }

        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    }
}
