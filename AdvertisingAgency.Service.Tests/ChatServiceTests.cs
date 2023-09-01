using AdvertisingAgency.Data.Data.Models.Chat;
using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Services;
using Microsoft.EntityFrameworkCore;
using AdvertisingAgency.Data.Data.Models;

namespace AdvertisingAgency.Service.Tests
{
    [TestFixture]
    public class ChatServiceTests
    {
        private ApplicationDbContext _context;
        private IChatService _service;
        private readonly string _testUserId = Guid.NewGuid().ToString();

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _service = new ChatService(_context);
        }

        [Test]
        public async Task Create_CreatesNewChatMessage()
        {
            var text = "Test message";

            await _service.Create(text, _testUserId);

            var message = _context.ChatMessages.FirstOrDefault(m => m.Text == text && m.UserId == _testUserId);
            Assert.IsNotNull(message);
            Assert.AreEqual(text, message.Text);
            Assert.AreEqual(_testUserId, message.UserId);
        }

        [Test]
        public async Task GetMessages_ReturnsMessagesOrderedByCreatedDate()
        {
            // Arrange - set up some test data
            var message1 = new ChatMessage { Text = "Test1", UserId = _testUserId, CreatedOn = DateTime.UtcNow.AddDays(-1) };
            var message2 = new ChatMessage { Text = "Test2", UserId = _testUserId, CreatedOn = DateTime.UtcNow };

            _context.ChatMessages.Add(message1);
            _context.ChatMessages.Add(message2);
            await _context.SaveChangesAsync();

            // Act
            var result = (await _service.GetMessages()).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Test2", result[0].Text);
            Assert.AreEqual("Test1", result[1].Text);
        }

        [Test]
        public async Task GetNewMessagesCount_ReturnsCorrectCount()
        {
            var message1 = new ChatMessage { Text = "Test1", UserId = _testUserId, CreatedOn = DateTime.UtcNow.AddDays(-1), Read = false };
            var message2 = new ChatMessage { Text = "Test2", UserId = "another_user_id", CreatedOn = DateTime.UtcNow, Read = false };

            _context.ChatMessages.Add(message1);
            _context.ChatMessages.Add(message2);
            await _context.SaveChangesAsync();

            var count = await _service.GetNewMessagesCount(_testUserId);

            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task GetMessages_ReturnsUserImage_WhenUserHasImage()
        {
            // Arrange - set up some test data
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User", ImageUrl = "https://cardesigner.blob.core.windows.net/cardesigner/78b98e2d-5217-4c45-a397-eec067b11fdd_DIMO.jpg" };

            _context.Users.Add(user);

            var message = new ChatMessage
            {
                Text = "TestMessage",
                UserId = user.Id,
                CreatedOn = DateTime.UtcNow
            };
            _context.ChatMessages.Add(message);

            await _context.SaveChangesAsync();

            // Act
            var result = (await _service.GetMessages()).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            var returnedMessage = result.First();
            Assert.AreEqual(user.ImageUrl, returnedMessage.UserImageUrl);
            Assert.AreEqual(message.CreatedOn, returnedMessage.CreatedOn);
        }

        [Test]
        public async Task GetMessages_ReturnsDefaultImage_WhenUserHasNoImage()
        {
            // Arrange - set up some test data
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User" };

            _context.Users.Add(user);

            var message = new ChatMessage
            {
                Text = "TestMessage",
                UserId = _testUserId,
                CreatedOn = DateTime.UtcNow
            };
            _context.ChatMessages.Add(message);

            await _context.SaveChangesAsync();

            // Act
            var result = (await _service.GetMessages()).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            var returnedMessage = result.First();
            Assert.AreEqual("/images/Profile-Avatar.png", returnedMessage.UserImageUrl); // Default image should be returned
            Assert.AreEqual(message.CreatedOn, returnedMessage.CreatedOn);
        }



        [Test]
        public async Task MarkAsRead_MarksMessagesAsRead()
        {
            var message1 = new ChatMessage { Text = "Test1", UserId = "another_user_id", CreatedOn = DateTime.UtcNow.AddDays(-1), Read = false };
            var message2 = new ChatMessage { Text = "Test2", UserId = "another_user_id", CreatedOn = DateTime.UtcNow, Read = false };

            _context.ChatMessages.Add(message1);
            _context.ChatMessages.Add(message2);
            await _context.SaveChangesAsync();

            await _service.MarkAsRead(_testUserId);

            var unreadMessages = _context.ChatMessages.Where(m => !m.Read).ToList();
            Assert.AreEqual(0, unreadMessages.Count);
        }
    }
}
