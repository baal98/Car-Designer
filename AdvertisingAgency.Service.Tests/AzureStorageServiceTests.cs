using AdvertisingAgency.Data.Common;
using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Services.AzureStorage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Imaging;
using System.Drawing;
using Xunit;

namespace AdvertisingAgency.Service.Tests
{
    public class AzureStorageServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AzureStorageConfig _azureStorageConfig;
        private readonly string _containerName;
        private readonly IAzureStorageService _azureStorageService;

        public AzureStorageServiceTests()
        {
            // Set up the in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Configure the Azure Storage service
            _azureStorageConfig = new AzureStorageConfig
            {
                //these tests will work with correct connection string and account name/key
                ConnectionString = "fake_connection_string",
                AccountName = "fake_account_name",
                AccountKey = "fake_account_key"
                //these are Configure the Azure Storage service test
            };
            _containerName = "cardesigner";
            _azureStorageService = new AzureStorageService(_azureStorageConfig, _containerName);
        }

        public void Dispose()
        {
            // Clean up the in-memory database after each test
            _context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task UploadFileBlobAsync_ShouldReturnBlobUrl_WhenUploadIsSuccessful()
        {
            // Arrange
            byte[] fileBytes = new byte[100];
            string fileName = "testfile.txt";
            string contentType = "text/plain";

            // Act
            string result = await _azureStorageService.UploadFileBlobAsync(fileBytes, fileName, contentType);

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

        [Test]
        public async Task DeleteBlobDataAsync_ShouldDeleteUploadedImage()
        {
            // Arrange
            byte[] imageBytes;
            string fileName = "testimage.jpg";
            string contentType = "image/jpeg";

            using (MemoryStream ms = new MemoryStream())
            {
                // Създайте тестова картинка и запишете я в MemoryStream
                using (Bitmap bitmap = new Bitmap(100, 100))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.Clear(Color.Red);
                    }

                    bitmap.Save(ms, ImageFormat.Jpeg);
                }

                imageBytes = ms.ToArray();
            }

            // Act
            var uri = await _azureStorageService.UploadFileBlobAsync(imageBytes, fileName, contentType);

            // Assert - проверяваме, че картинката е качена успешно
            uri.Should().NotBeNullOrEmpty();
            Uri resultUri;
            bool isValidUri = Uri.TryCreate(uri, UriKind.Absolute, out resultUri);
            isValidUri.Should().BeTrue();

            // Act - изтриваме качената картинка
            await _azureStorageService.DeleteBlobDataAsync(uri);

            // Assert - проверяваме, че картинката е изтрита успешно
            using (HttpClient httpClient = new HttpClient())
            {
                // Проверяваме дали URL адресът вече не съществува
                HttpResponseMessage response = await httpClient.GetAsync(uri);
                response.IsSuccessStatusCode.Should().BeFalse();
            }
        }


        [Test]
        public void UploadFileBlobAsync_ShouldUploadImageAndReturnValidUri()
        {
            // Arrange
            byte[] imageBytes;
            string fileName = "testimage.jpg";
            string contentType = "image/jpeg";

            using (MemoryStream ms = new MemoryStream())
            {
                // Създайте тестова картинка и запишете я в MemoryStream
                using (Bitmap bitmap = new Bitmap(100, 100))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.Clear(Color.Red);
                    }

                    bitmap.Save(ms, ImageFormat.Jpeg);
                }

                imageBytes = ms.ToArray();
            }

            // Act
            var uri = _azureStorageService.UploadFileBlobAsync(imageBytes, fileName, contentType).Result;

            // Assert
            uri.Should().NotBeNullOrEmpty();
            Uri resultUri;
            bool isValidUri = Uri.TryCreate(uri, UriKind.Absolute, out resultUri);
            isValidUri.Should().BeTrue();
        }

        [Fact]
        public void GenerateSasToken_ShouldReturnValidSasToken_WhenBlobNameIsValid()
        {
            // Arrange
            string blobName = "testfile.txt";

            // Act
            string sasToken = _azureStorageService.GenerateSasToken(blobName);

            // Assert
            sasToken.Should().NotBeNullOrEmpty();
            sasToken.Should().Contain($"sig=");
            sasToken.Should().Contain($"se=");
            sasToken.Should().Contain($"sp=r");
            sasToken.Should().Contain($"sv=");
        }
    }
}
