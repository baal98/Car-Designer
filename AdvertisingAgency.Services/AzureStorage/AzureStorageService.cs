using AdvertisingAgency.Data.Common;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace AdvertisingAgency.Services.AzureStorage
{
    /// <summary>
    /// Provides functionalities to interact with Azure Blob Storage, including uploading, deleting, and generating Shared Access Signatures (SAS).
    /// </summary>
    public class AzureStorageService : IAzureStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly AzureStorageConfig _azureStorageConfig;
        public BlobContainerClient ContainerClient => _containerClient;

        /// <summary>
        /// Initializes a new instance of the AzureStorageService class with the provided configuration and container name.
        /// </summary>
        /// <param name="azureStorageConfig">The Azure Storage configuration settings.</param>
        /// <param name="containerName">The name of the container within the storage account.</param>
        public AzureStorageService(AzureStorageConfig azureStorageConfig, string containerName)
        {
            _azureStorageConfig = azureStorageConfig;
            var blobServiceClient = new BlobServiceClient(_azureStorageConfig.ConnectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Add this line to create the container if it does not exist
            _containerClient.CreateIfNotExistsAsync().Wait();
        }

        //метод за пост в Azure Blob Storage с 1 аргумент, без преоразмеряване на картинката в оригиналния вид - работи с аналогичния метод от Register модела!!!

        //public async Task<string> UploadFileBlobAsync(IFormFile file)
        //{
        //    var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        //    var blobClient = _containerClient.GetBlobClient(uniqueFileName);

        //    using (var stream = file.OpenReadStream())
        //    {
        //        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType } });
        //    }

        //    return blobClient.Uri.ToString();
        //}

        /// <summary>
        /// Uploads a file as a blob to the Azure Storage container.
        /// </summary>
        /// <param name="fileBytes">The byte array of the file to be uploaded.</param>
        /// <param name="fileName">The original name of the file.</param>
        /// <param name="contentType">The MIME type of the file.</param>
        /// <returns>The URL of the uploaded blob.</returns>
        public async Task<string> UploadFileBlobAsync(byte[] fileBytes, string fileName, string contentType)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
            var blobClient = _containerClient.GetBlobClient(uniqueFileName);

            using (var stream = new MemoryStream(fileBytes))
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } });
            }

            return blobClient.Uri.ToString();
        }

        /// <summary>
        /// Deletes a blob from the Azure Storage container.
        /// </summary>
        /// <param name="blobUrl">The URL of the blob to be deleted.</param>
        public async Task DeleteBlobDataAsync(string blobUrl)
        {
            var uri = new Uri(blobUrl);
            var blobName = uri.Segments.Last();
            var blobClient = _containerClient.GetBlobClient(blobName);

            // Delete the blob
            await blobClient.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Generates a Shared Access Signature (SAS) token for a specific blob.
        /// </summary>
        /// <param name="blobName">The name of the blob for which to generate the SAS token.</param>
        /// <returns>The URL of the blob along with the generated SAS token.</returns>
        public string GenerateSasToken(string blobName)
        {
            var blobServiceClient = new BlobServiceClient(_azureStorageConfig.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerClient.Name);
            var blobClient = containerClient.GetBlobClient(blobName);

            var builder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b", // b stands for blob
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1) // SAS token will be valid for 1 hour
            };

            builder.SetPermissions(BlobSasPermissions.Read); // Setting the permissions to Read only

            var sasToken = builder.ToSasQueryParameters(new StorageSharedKeyCredential(_azureStorageConfig.AccountName, _azureStorageConfig.AccountKey)).ToString();

            return blobClient.Uri + "?" + sasToken;
        }
    }
}
