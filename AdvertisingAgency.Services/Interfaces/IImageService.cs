namespace AdvertisingAgency.Services.Interfaces
{
    public interface IImageService
    {
        byte[] ResizeImage(byte[] imageBytes, int targetSize);
    }
}
