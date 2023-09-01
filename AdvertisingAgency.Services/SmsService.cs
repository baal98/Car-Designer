using AdvertisingAgency.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using TwilioNumber = Twilio.Types.PhoneNumber;

namespace AdvertisingAgency.Services
{
    /// <summary>
    /// Provides functionality for sending SMS messages via Twilio.
    /// </summary>
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration used for Twilio credentials.</param>
        public SmsService(IConfiguration configuration)
        {
            _configuration = configuration;

            string accountSid = _configuration["Twilio:AccountSID"];
            string authToken = _configuration["Twilio:AuthToken"];
            TwilioClient.Init(accountSid, authToken);
        }

        /// <summary>
        /// Sends an SMS message to the specified phone number.
        /// </summary>
        /// <param name="to">The phone number to send the SMS to.</param>
        /// <param name="body">The content of the SMS message.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public async Task SendSms(string to, string body)
        {
            string from = _configuration["Twilio:PhoneNumber"];

            await MessageResource.CreateAsync(
                to: new TwilioNumber(to),
                from: new TwilioNumber(from),
                body: body
            );
        }
    }
}