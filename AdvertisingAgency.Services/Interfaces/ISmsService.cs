namespace AdvertisingAgency.Services.Interfaces
{
    public interface ISmsService
    {
        public Task SendSms(string to, string body);
    }
}
