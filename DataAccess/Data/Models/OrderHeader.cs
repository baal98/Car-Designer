using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace AdvertisingAgency.Data.Data.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }
        
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime ShippingDate { get; set; }

        public double OrderTotal { get; set; }

        public string? OrderStatus { get; set; }

        public string? PaymentStatus { get; set; }

        public string? TrackingNumber { get; set; }

        public string? Carrier { get; set; }

        public DateTime PaymentDate { get; set; }

        public DateTime PaymentDueDate { get; set; }

        public string? SessionId { get; set; }

        public string? PaymentIntendId { get; set; }

        public string? PhoneNumber { get; set; }

        [MaxLength(DataConstants.AddressStreetMaxLength)]
        [MinLength(DataConstants.AddressStreetMinLength)]
        public string? Street { get; set; }

        public int? CityId { get; set; }

        public City? City { get; set; }

        public int? CountryId { get; set; }
        public Country? Country { get; set; }

        public string? PostalCode { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        [MaxLength(DataConstants.AddressBuildingNumberMaxLength)]
        [MinLength(DataConstants.AddressStreetMinLength)]
        public string? BuildingNumber { get; set; }

        [MaxLength(DataConstants.AdditionalInfoMaxLength)]
        [MinLength(DataConstants.AdditionalInfoMinLength)]
        public string? AdditionalInfo { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    }
}