namespace MoviesAdmin.Services
{
    public interface IAzureStorageService
    {
        // Upload a file to Azure Blob Storage (Azurite while developing)
        // return: the URL of the uploaded file as a string
        public Task<string> UploadFileAsync(IFormFile file, string fileName);

        // Delete the file a URL points to (when a movie is deleted or its poster is replaced)
        // Does nothing if the URL is empty or the file is already gone.
        public Task DeleteFileAsync(string fileUrl);
    }
}