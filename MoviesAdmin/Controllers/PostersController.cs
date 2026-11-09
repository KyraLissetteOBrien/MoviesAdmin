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
            try
            {
                blobName = Path.GetFileName(blobName);

                var file = await _storage.GetFileAsync(blobName);
                if (file == null) return NotFound();

                return File(file.Value.Content, file.Value.ContentType);
            }
            catch (Exception ex)
            {
                return Content(ex.GetType().Name + ": " + ex.Message);
            }
        }
    }
}