using AdvertisingAgency.Data.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvertisingAgency.Common.DTOs
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
