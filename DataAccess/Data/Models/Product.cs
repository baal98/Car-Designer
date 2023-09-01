using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AdvertisingAgency.Data.Data.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }

        [Required]
        public string? Author { get; set; }

        [Range(1, 1000)]
        [Display(Name = "Price")]
        public double Price { get; set; }

        [ValidateNever]
        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        [Display(Name = "Category")]
        [ValidateNever]
        public string? Category { get; set; }

        public int Count { get; set; }

        public Guid CanvasId { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
