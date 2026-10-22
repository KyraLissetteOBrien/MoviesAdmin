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
            // get client for containerName (e.g. "uploads")
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Automatically creates the container directory in your new Docker/Azurite container if missing.
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);

            // set the content type so the browser shows the image instead of downloading it
            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            };

            // file I/O
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, options); // pass in options when uploading
            }

            // return the URL for the saved file
            return blobClient.Uri.ToString();

        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl) || !Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
            {
                return;
            }

            // the blob name is the last part of the URL, e.g. 3f2b9c1e-....jpg
            string blobName = Path.GetFileName(Uri.UnescapeDataString(uri.AbsolutePath));

            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // DeleteIfExists so a missing file doesn't crash the delete/edit
            await containerClient.GetBlobClient(blobName).DeleteIfExistsAsync();
        }
    }
}