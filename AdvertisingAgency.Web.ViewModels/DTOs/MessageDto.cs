using AdvertisingAgency.Data.Data;
using System.ComponentModel.DataAnnotations;

namespace AdvertisingAgency.Web.ViewModels.DTOs
{
    using static DataConstants;

    public class MessageDto
    {
        [MaxLength(MessageMaxLength)]
        public string Text { get; set; }

        [MaxLength(UserFullNameMaxLength)]
        public string Username { get; set; }

        [Url]
        public string UserImageUrl { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
