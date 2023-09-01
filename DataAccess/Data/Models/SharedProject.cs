using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvertisingAgency.Data.Data.Models
{
    public class SharedProject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? SharingUserId { get; set; }
        public ApplicationUser? SharingUser { get; set; }

        public Guid? CollectingUserId { get; set; }

        public Guid? CanvasId { get; set; }

        [ForeignKey("CanvasId")]
        public Canvas? Canvas { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
