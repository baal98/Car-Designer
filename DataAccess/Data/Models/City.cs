using System.ComponentModel.DataAnnotations;
using static AdvertisingAgency.Data.Data.DataConstants;

namespace AdvertisingAgency.Data.Data.Models
{
    public class City
    {
        public int Id { get; init; }

        [MinLength(CityMinLength)]
        [MaxLength(CityMaxLength)]
        public string Name { get; set; }

        [Required]
        [MinLength(CityMinLength)]
        [MaxLength(CityMaxLength)]
        public int CountryId { get; set; }
        public Country? Country { get; set; }

        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        public ICollection<Address> Addresses { get; set; } = new HashSet<Address>();
    }
}
