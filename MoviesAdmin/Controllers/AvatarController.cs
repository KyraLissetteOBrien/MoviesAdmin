using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesAdmin.Services;

namespace MoviesAdmin.Controllers
{
    [Authorize]
    public class AvatarController : Controller
    {
        private readonly IAzureStorageService _storage;
        public AvatarController(IAzureStorageService storage) { _storage = storage; }

        private string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private static string BlobName(string userId) =>
            "avatar-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userId)))[..16] + ".jpg";

        [HttpPost("/avatar/upload")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile photo)
        {
            if (UserId == null || photo == null || photo.Length == 0 ||
                photo.Length > 2_000_000 || !photo.ContentType.StartsWith("image/"))
                return BadRequest("Choose an image under 2 MB.");

            var name = BlobName(UserId);
            await _storage.DeleteFileAsync(name);   // allows replacing
            await _storage.UploadFileAsync(photo, name);
            return Redirect(Request.Headers.Referer.FirstOrDefault() ?? "/");
        }

        [HttpGet("/avatar/me")]
        public async Task<IActionResult> Me()
        {
            if (UserId == null) return NotFound();
            var file = await _storage.GetFileAsync(BlobName(UserId));
            if (file == null) return NotFound();
            return File(file.Value.Content, file.Value.ContentType);
        }
    }
}