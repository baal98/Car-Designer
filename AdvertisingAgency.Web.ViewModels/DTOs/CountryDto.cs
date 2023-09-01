using AdvertisingAgency.Data.Data;
using System.ComponentModel.DataAnnotations;

namespace AdvertisingAgency.Web.ViewModels.DTOs
{
    using static DataConstants;

    public class CountryDto
    {
        public int Id { get; set; }

        [MaxLength(CountryNameMaxLength)]
        public string Name { get; set; }
        public string CountryCode { get; set; }

        public List<CityDto> Cities { get; set; }
    }
}
