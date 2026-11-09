using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace MoviesAdmin.Services
{
    public class AzureStorageService : IAzureStorageService
    {
        private readonly IConfiguration _configuration;

        private readonly BlobServiceClient blobServiceClient;
        private string containerName;

        // Constructor
        public AzureStorageService(IConfiguration configuration)
        {
            // configuration is used to get appsettings, user-secrets, env vars, etc.
            _configuration = configuration;

            // get Azure Storage container name (appsettings.json)
            containerName = _configuration["AzureStorage:ContainerName"]
                ?? throw new InvalidOperationException("Missing configuration: AzureStorage:ContainerName");

            // get Azure Storage connection string (kept in user-secrets, not in the repo)
            string connectionString = _configuration["AzureStorage:ConnectionString"] ?? "";
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Missing configuration: AzureStorage:ConnectionString. Add it to user-secrets (right-click the project > Manage User Secrets).");
            }

            // initialize Azure blob client
            blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // creates a PRIVATE container if it doesn't exist yet
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);

            // set the content type so the browser shows the image instead of downloading it
            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            };

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, options);
            }

            // relative URL, served by the app (see PostersController, next step)
            return $"/posters/{fileName}";
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return;

            // works for both "/posters/abc.jpg" and full blob URLs
            string blobName = Path.GetFileName(Uri.UnescapeDataString(fileUrl));

            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.GetBlobClient(blobName).DeleteIfExistsAsync();
        }

        public async Task<(Stream Content, string ContentType)?> GetFileAsync(string blobName)
        {
            var blobClient = blobServiceClient
                .GetBlobContainerClient(containerName)
                .GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync()) return null;

            var result = await blobClient.DownloadStreamingAsync();
            return (result.Value.Content, result.Value.Details.ContentType);
        }
    }
}