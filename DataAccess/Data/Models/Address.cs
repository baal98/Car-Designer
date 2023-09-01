using System.ComponentModel.DataAnnotations;

namespace AdvertisingAgency.Data.Data.Models
{
    public class Address
    {
        public int Id { get; set; }

        public int? CityId { get; set; }
        public City? City { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int? CountryId { get; set; }
        public Country? Country { get; set; }

        [MaxLength(DataConstants.AddressStreetMaxLength)]
        [MinLength(DataConstants.AddressStreetMinLength)]
        public string? Street { get; set; }

        [MaxLength(DataConstants.AddressBuildingNumberMaxLength)]
        [MinLength(DataConstants.AddressBuildingNumberMinLength)]
        public string? BuildingNumber { get; set; }

        [MaxLength(DataConstants.AdditionalInfoMaxLength)]
        [MinLength(DataConstants.AdditionalInfoMinLength)]
        public string? AdditionalInfo { get; set; }
    }
}
