using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdvertisingAgency.Data.Data.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        [Range(1, 100, ErrorMessage = "Please enter a value between 1 and 1000")]
        public int Count { get; set; }

        public string ApplicationUserId { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public string? Description { get; set; }

        public string? Title { get; set; }

        [NotMapped]
        public double Price { get; set; }

        [Url]
        public string? ImageUrl { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
