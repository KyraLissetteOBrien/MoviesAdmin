using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesAdmin.Services;

namespace MoviesAdmin.Controllers
{
    [Authorize]
    public class PostersController : Controller
    {
        private readonly IAzureStorageService _storage;

        public PostersController(IAzureStorageService storage)
        {
            _storage = storage;
        }

        [HttpGet("/posters/{blobName}")]
        public async Task<IActionResult> Get(string blobName)
        {
            // strip any path pieces so only a plain file name is used
            blobName = Path.GetFileName(blobName);

            var file = await _storage.GetFileAsync(blobName);
            if (file == null) return NotFound();

            return File(file.Value.Content, file.Value.ContentType);
        }
    }
}