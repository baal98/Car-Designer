//using NUnit.Framework;
//using Moq;
//using Microsoft.Extensions.Configuration;
//using System.Threading.Tasks;
//using Twilio;
//using Twilio.Rest.Api.V2010.Account;
//using AdvertisingAgency.Services.Interfaces;
//using Twilio.Clients;

//namespace AdvertisingAgency.Services.Tests
//{
//    [TestFixture]
//    public class SmsServiceTests
//    {
//        private Mock<IConfiguration> configurationMock;

//        [SetUp]
//        public void SetUp()
//        {
//            configurationMock = new Mock<IConfiguration>();
//            configurationMock.SetupGet(m => m["Twilio:AccountSID"]).Returns("your_account_sid");
//            configurationMock.SetupGet(m => m["Twilio:AuthToken"]).Returns("your_auth_token");
//            configurationMock.SetupGet(m => m["Twilio:PhoneNumber"]).Returns("your_twilio_phone_number");
//        }

//        [Test]
//        public async Task SendSms_ValidInput_CallsTwilioApiWithCorrectParameters()
//        {
//            // Arrange
//            string to = "+123456789"; // Replace with the actual phone number
//            string body = "Test SMS body";

//            var smsService = new SmsService(configurationMock.Object);

//            var twilioClientMock = new Mock<ITwilioRestClient>();
//            TwilioClient.SetRestClient(twilioClientMock.Object);

//            // Act
//            await smsService.SendSms(to, body);

//            // Assert
//            twilioClientMock.Verify(
//                m => m.ExecuteAsync(
//                    It.Is<RestRequest>(r =>
//                        r.Resource == "/2010-04-01/Accounts/your_account_sid/Messages.json" &&
//                        r.Method == Twilio.Http.HttpMethod.Post &&
//                        r.FormParams["To"] == to &&
//                        r.FormParams["From"] == "your_twilio_phone_number" &&
//                        r.FormParams["Body"] == body
//                    ),
//                    null
//                ),
//                Times.Once
//            );
//        }

//        [Test]
//        public void SendSms_NullTo_ThrowsArgumentNullException()
//        {
//            // Arrange
//            string to = null;
//            string body = "Test SMS body";

//            var smsService = new SmsService(configurationMock.Object);

//            // Act and Assert
//            Assert.ThrowsAsync<ArgumentNullException>(async () => await smsService.SendSms(to, body));
//        }

//        // Add more test methods for other scenarios, like invalid phone number, empty body, etc.
//    }
//}
