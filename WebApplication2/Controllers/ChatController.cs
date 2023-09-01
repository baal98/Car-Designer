using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AdvertisingAgency.Web.Controllers
{
    using AdvertisingAgency.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a controller for managing chat-related operations.
    /// </summary>
    [Authorize]
    public class ChatController : BaseController
    {
        private readonly IChatService chat;

        /// <summary>
        /// Initializes a new instance of the ChatController class with the specified chat service.
        /// </summary>
        /// <param name="chat">The service responsible for chat-related operations.</param>
        public ChatController(IChatService chat)
            => this.chat = chat;

        /// <summary>
        /// Displays the chat view.
        /// </summary>
        /// <returns>The view containing chat messages.</returns>
        public async Task<IActionResult> Chat()
        {
            if (User.Identity == null)
            {
                return this.View(await this.chat.GetMessages());
            }
            ViewBag.CurrentUsername = User.Identity.Name;
            var messages = await this.chat.GetMessages();

            return this.View(messages);
        }

        /// <summary>
        /// Retrieves chat messages as JSON.
        /// </summary>
        /// <returns>A JSON result containing chat messages.</returns>
        [HttpGet]
        public async Task<JsonResult> GetChatMessages()
        {
            var messages = await this.chat.GetMessages();
            return Json(messages);
        }

        /// <summary>
        /// Retrieves the count of new chat messages as JSON.
        /// </summary>
        /// <returns>A JSON result containing the count of new chat messages.</returns>
        [HttpGet]
        public async Task<JsonResult> GetNewMessagesCount()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var newMessagesCount = await this.chat.GetNewMessagesCount(userId);
            return Json(newMessagesCount);
        }

        /// <summary>
        /// Marks chat messages as read for the current user.
        /// </summary>
        /// <returns>An IActionResult indicating the result of marking messages as read.</returns>
        [HttpPost]
        public async Task<IActionResult> MarkAsRead()
        {
            var currentUserId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await this.chat.MarkAsRead(currentUserId);
            return this.Ok();
        }
    }
}
