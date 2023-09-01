using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Data.Data.Models.Chat;

namespace AdvertisingAgency.Services
{
    using AdvertisingAgency.Services.Interfaces;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext data;

        public ChatService(ApplicationDbContext data)
        {
            this.data = data;
        }

        /// <summary>
        /// Creates and stores a new chat message in the database.
        /// </summary>
        /// <param name="text">The content of the chat message.</param>
        /// <param name="userId">The ID of the user sending the message.</param>
        public async Task Create(string text, string userId)
        {
            await data.AddAsync(new ChatMessage
            {
                Text = text,
                UserId = userId,
                CreatedOn = DateTime.UtcNow,
            });
            await data.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a list of chat messages, ordered by creation date, and maps them to Message DTOs.
        /// </summary>
        /// <returns>A list of mapped chat messages.</returns>
        public async Task<IEnumerable<Message>> GetMessages()
        {
            var chatMessages = await data.ChatMessages
                .OrderByDescending(message => message.CreatedOn)
                .ToListAsync();

            var userIds = chatMessages.Select(message => message.UserId).Distinct().ToList();

            var users = await data.Users.OfType<ApplicationUser>().Where(user => userIds.Contains(user.Id)).ToListAsync();

            var mappedMessages = chatMessages.Select(message => new Message
            {
                Text = message.Text,
                Username = users.FirstOrDefault(user => user.Id == message.UserId)?.UserName,
                UserImageUrl = users.FirstOrDefault(u => u.Id == message.UserId)?.ImageUrl ?? "/images/Profile-Avatar.png",
                CreatedOn = message.CreatedOn
            }).ToList();



            return mappedMessages;
        }

        /// <summary>
        /// Counts the number of new messages for a user that haven't been read.
        /// </summary>
        /// <param name="userId">The ID of the user to check for new messages.</param>
        /// <returns>The count of unread messages not sent by the user.</returns>
        public async Task<int> GetNewMessagesCount(string userId)
        {
            var newMessagesCount = await data.ChatMessages
                .Where(message => message.UserId != userId && !message.Read)
                .CountAsync();
            return newMessagesCount;
        }

        /// <summary>
        /// Marks all unread messages (not sent by the user) as read.
        /// </summary>
        /// <param name="userId">The ID of the user marking the messages as read.</param>
        public async Task MarkAsRead(string userId)
        {
            var unreadMessages = data.ChatMessages
                .Where(message => message.UserId != userId && !message.Read);

            foreach (var message in unreadMessages)
            {
                message.Read = true;
            }

            await data.SaveChangesAsync();
        }
    }
}
