using System.ComponentModel.DataAnnotations;

namespace AdvertisingAgency.Data.Data.Models.Chat
{
    using System;
    using static DataConstants;


    public class Message
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
