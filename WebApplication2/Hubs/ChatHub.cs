using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Data.Data.Models.Chat;

namespace AdvertisingAgency.Web.Hubs
{
    using AdvertisingAgency.Services.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.SignalR;
    using System;
    using System.Threading.Tasks;

    [Authorize]
    public class ChatHub : Hub
    {
        private const int MessageMinLength = 2;
        private const int MessageMaxLength = 300;

        private readonly UserManager<ApplicationUser> userManager;
        private readonly IChatService chat;

        public ChatHub(UserManager<ApplicationUser> userManager, IChatService chat)
        {
            this.userManager = userManager;
            this.chat = chat;
        }

        /// <summary>
        /// Sends a message to the chat.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Send(string message)
        {
            if (message.Length < MessageMinLength || message.Length > MessageMaxLength)
            {
                return;
            }

            var user = await this.userManager.GetUserAsync(this.Context.User);
            await this.chat.Create(message, user.Id);

            await this.Clients.All.SendAsync(
                "NewMessage",
                new Message
                {
                    Text = message,
                    Username = user.UserName,
                    UserImageUrl = user.ImageUrl,
                    CreatedOn = DateTime.UtcNow,
                });
        }
    }
}
