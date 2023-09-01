using AdvertisingAgency.Data.Data.Models.Chat;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdvertisingAgency.Data.Data.Models
{
    using AdvertisingAgency.Data.Data.Models.Enums;
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using static DataConstants;

    public class ApplicationUser : IdentityUser<string>
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

        [MaxLength(UserFullNameMaxLength)]
        public string? FriendlyName { get; set; } = null!;

        [Url]
        public string? ImageUrl { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; } = null!;

        public int? CountryId { get; set; }
        public int? CityId { get; set; }

        public Country? Country { get; set; }
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

        public ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

        public ICollection<OrderHeader> OrderHeaders { get; set; } = new List<OrderHeader>();

    }
}
