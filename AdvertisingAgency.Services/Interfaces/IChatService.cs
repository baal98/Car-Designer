using AdvertisingAgency.Data.Data.Models.Chat;

namespace AdvertisingAgency.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IChatService
    {
        Task Create(string text, string userId);

        Task<IEnumerable<Message>> GetMessages();

        Task<int> GetNewMessagesCount(string userId);

        Task MarkAsRead(string userId);
    }
}
