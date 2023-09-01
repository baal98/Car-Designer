namespace AdvertisingAgency.Services.AzureStorage
{
    public interface IAzureStorageService
    {
        Task<string> UploadFileBlobAsync(byte[] fileBytes, string fileName, string contentType);

        string GenerateSasToken(string blobName);

        Task DeleteBlobDataAsync(string blobUrl);
    }
}
