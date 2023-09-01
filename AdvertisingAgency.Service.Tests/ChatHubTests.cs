using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Security.Claims;

namespace AdvertisingAgency.Service.Tests
{
    public class ChatHubTests
    {
        [Test]
        public async Task Send_ShouldBroadcastNewMessage_WhenMessageIsValid()
        {
            // Arrange
            var mockUser = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "testUserName", ImageUrl = "testUrl" };
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, mockUser.Id),
                new Claim(ClaimTypes.Name, mockUser.UserName)
            });
            var principal = new ClaimsPrincipal(identity);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            mockUserManager.Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(mockUser);

            var mockChatService = new Mock<IChatService>();
            var mockClients = new Mock<IHubCallerClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(context => context.User).Returns(principal);

            var chatHub = new ChatHub(mockUserManager.Object, mockChatService.Object)
            {
                Clients = mockClients.Object,
                Context = mockContext.Object
            };

            var message = "TestMessage";

            // Act
            await chatHub.Send(message);

            // Assert
            mockChatService.Verify(s => s.Create(message, mockUser.Id), Times.Once());
            mockClientProxy.Verify(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default), Times.Once());
        }
    }
}
