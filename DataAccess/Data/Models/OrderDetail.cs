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
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        [ValidateNever]
        public OrderHeader OrderHeader { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        public int Count { get; set; }

        public double Price { get; set; }

        public string? ProductTitle { get; set; }
        public string? ProductDescription { get; set; }
        public double? ProductPrice { get; set; }
    }
}
