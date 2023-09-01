using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Data.Data.Models.Chat;
using AdvertisingAgency.Data.Data.Models.Enums;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdvertisingAgency.Common.DTOs
{
    using static DataConstants;

    public class ApplicationUserDTO
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

        public string FriendlyName { get; set; } = null!;

        [Url]
        public string? ImageUrl { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; } = null!;

        [JsonProperty("PhoneNumber")]
        [Phone]
        public string PhoneNumber { get; set; } = null!;

        public int? CountryId { get; set; }
        public int? CityId { get; set; }

        public Country Country { get; set; }
        public City? City { get; set; }

        [ForeignKey("AddressId")]
        public int? AddressId { get; set; }

        [MinLength(AddressStreetMinLength)]
        [MaxLength(AddressStreetMaxLength)]
        public Address? Address { get; set; }

        public UserRole UserRole { get; set; }

        public Gender Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public bool IsAccountCompleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public IEnumerable<ChatMessage> ChatMessages { get; set; } = new HashSet<ChatMessage>();

        public ICollection<Canvas> Canvases { get; set; } = new HashSet<Canvas>();

        public ICollection<Address> Addresses { get; set; } = new HashSet<Address>();
    }
}
