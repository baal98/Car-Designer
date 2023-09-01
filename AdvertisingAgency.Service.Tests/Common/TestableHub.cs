using Microsoft.AspNetCore.SignalR;
using Moq;

namespace AdvertisingAgency.Service.Tests.Common
{
    public class TestableHub<T> : Hub<T> where T : class
    {
        public void Configure(ContextUser user)
        {
            var mock = new Mock<HubCallerContext>();
            mock.Setup(_ => _.User).Returns(user);
            Context = mock.Object;
        }
    }
}
