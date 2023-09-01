using AdvertisingAgency.Services.Interfaces;

namespace AdvertisingAgency.Services
{
    /// <summary>
    /// Provides services related to image processing.
    /// </summary>
    public class ImageService : IImageService
    {
        /// <summary>
        /// Resizes an image to a target size while maintaining its aspect ratio. The resulting image is saved as a JPEG.
        /// </summary>
        /// <param name="imageBytes">The byte array representation of the image to be resized.</param>
        /// <param name="targetSize">The target size for the image's larger dimension (width or height).</param>
        /// <returns>A byte array containing the resized JPEG image.</returns>
        public byte[] ResizeImage(byte[] imageBytes, int targetSize)
        {
            using var inputStream = new MemoryStream(imageBytes);
            using var image = Image.Load(inputStream);
            var scaleFactor = (double)targetSize / Math.Max(image.Width, image.Height);
            var newWidth = (int)Math.Round(image.Width * scaleFactor);
            var newHeight = (int)Math.Round(image.Height * scaleFactor);
            image.Mutate(x => x.Resize(newWidth, newHeight));
            var outputStream = new MemoryStream();
            image.SaveAsJpeg(outputStream);
            return outputStream.ToArray();
        }
    }
}