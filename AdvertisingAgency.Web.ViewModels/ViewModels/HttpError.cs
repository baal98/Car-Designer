namespace AdvertisingAgency.Web.ViewModels.ViewModels
{
    public class HttpError
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public HttpError(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
