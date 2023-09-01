using System.ComponentModel.DataAnnotations;
using AdvertisingAgency.Data.Data;

namespace AdvertisingAgency.Web.ViewModels.DTOs
{
    using static DataConstants;

    public class CityDto
    {
        public int Id { get; set; }

        [MaxLength(UserFullNameMaxLength)]
        public string Name { get; set; }
        public int CountryId { get; set; }
        public CountryDto Country { get; set; }

        public List<UserDto> Users { get; set; }
    }
}
