using AdvertisingAgency.Services;
using AdvertisingAgency.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AdvertisingAgency.Service.Tests
{
    [TestFixture]
    public class SmsServiceTests
    {
        private Mock<IConfiguration> _configurationMock;
        private ISmsService _smsService;
        private const string AccountSid = "AccountSID";
        private const string AuthToken = "AuthToken";
        private const string PhoneNumber = "PhoneNumber";

        [SetUp]
        public void SetUp()
        {
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["Twilio:AccountSID"]).Returns(AccountSid);
            _configurationMock.Setup(c => c["Twilio:AuthToken"]).Returns(AuthToken);
            _configurationMock.Setup(c => c["Twilio:PhoneNumber"]).Returns(PhoneNumber);
            _smsService = new SmsService(_configurationMock.Object);
        }

        [Test]
        public void Constructor_ShouldInitializeTwilioClient()
        {
            _smsService.Should().NotBeNull();
        }
    }
}
