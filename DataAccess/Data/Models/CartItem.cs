using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvertisingAgency.Data.Data.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
        public int Quantity { get; set; }

        public Guid? CanvasId { get; set; }

        public int ShoppingCartId { get; set; }
        [ForeignKey("ShoppingCartId")]
        public ShoppingCart ShoppingCart { get; set; }

        public string? Thumbnail { get; set; }
    }
}
