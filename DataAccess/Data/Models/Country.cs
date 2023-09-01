namespace AdvertisingAgency.Data.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using static DataConstants;

    public class Country
    {
        public int Id { get; init; }

        [Required]
        [MaxLength(CountryNameMaxLength)]
        public string Name { get; init; }

        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        public List<City> Cities { get; set; } = new List<City>();

        public ICollection<Address> Addresses { get; set; } = new HashSet<Address>();
    }
}
