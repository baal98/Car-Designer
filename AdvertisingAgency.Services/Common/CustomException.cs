namespace AdvertisingAgency.Services.Common
{
    public class CustomException : Exception
    {
        public CustomException(string message)
            : base(message)
        {
        }
    }
}
