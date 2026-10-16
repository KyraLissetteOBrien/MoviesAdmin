using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Models;
using MoviesAdmin.Services;

public class MoviesController : Controller
{
    private readonly MoviesAdminContext _context;
    private readonly AzureStorageService _blobService;

    // Poster rules shown to the user as validation messages
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB

    // Constructor - the framework "injects" the database context and the upload service (Dependency Injection)
    public MoviesController(MoviesAdminContext context, AzureStorageService blobService)
    {
        _context = context;
        _blobService = blobService;
    }

    // GET: Movies
    public async Task<IActionResult> Index()
    {
        var movies = await _context.Movie
            .OrderBy(m => m.ReleaseDate)
            .ToListAsync();

        return View(movies);
    }

    // GET: Movies/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movie
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    // GET: Movies/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Movies/Create
    // The form is "multipart/form-data", so the text fields are bound to "movie"
    // and the chosen file arrives as "imageFile" (the name of the <input type="file">).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Title,Description,Runtime,Genres,Rating,Synopsis,ReleaseDate,FunFact,Directors")] Movie movie,
        IFormFile? imageFile)
    {
        // an image is REQUIRED when creating a movie - give the user feedback if it is missing or not allowed
        if (imageFile == null || imageFile.Length == 0)
        {
            ModelState.AddModelError("imageFile", "Please choose a poster image.");
        }
        else if (CheckImage(imageFile) is string imageError)
        {
            ModelState.AddModelError("imageFile", imageError);
        }

        // ImageUrl is filled in by the server below, not typed into the form, so don't validate it
        ModelState.Remove(nameof(Movie.ImageUrl));

        if (ModelState.IsValid)
        {
            // save file in Azure storage using a unique (GUID) name so files never overwrite each other
            string imageUrl = await _blobService.UploadFileAsync(imageFile!, NewFileName(imageFile!));

            // capture the URL as part of the movie record
            movie.ImageUrl = imageUrl;

            // save record in database
            _context.Add(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(movie);
    }

    // GET: Movies/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movie.FindAsync(id);

        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    // POST: Movies/Edit/5
    // Changing the poster is OPTIONAL: if no new file is chosen, the existing image stays.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id,
        [Bind("Id,Title,Description,Genres,Rating,Runtime,Synopsis,ReleaseDate,FunFact,Directors")] Movie movie,
        IFormFile? imageFile)
    {
        if (id != movie.Id)
        {
            return NotFound();
        }

        // only validate the file if the user actually picked a new one
        bool replacingImage = imageFile != null && imageFile.Length > 0;
        if (replacingImage && CheckImage(imageFile!) is string imageError)
        {
            ModelState.AddModelError("imageFile", imageError);
        }

        // ImageUrl is not part of the form, so don't validate it
        ModelState.Remove(nameof(Movie.ImageUrl));

        // read what is currently saved (AsNoTracking = read only copy) so editing the title
        // doesn't wipe the image URL or the created date, which the form doesn't send back
        var existing = await _context.Movie.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (existing == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            movie.ImageUrl = existing.ImageUrl;
            movie.CreatedDate = existing.CreatedDate;

            if (replacingImage)
            {
                // upload the new poster and store its URL on the record
                movie.ImageUrl = await _blobService.UploadFileAsync(imageFile!, NewFileName(imageFile!));
            }

            try
            {
                _context.Update(movie);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(movie.Id))
                {
                    return NotFound();
                }

                throw;
            }

            // only after the database is saved, remove the OLD poster so no orphaned file is left
            if (replacingImage)
            {
                await _blobService.DeleteFileAsync(existing.ImageUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // redisplay the form: put the saved image URL back so the current poster still shows
        movie.ImageUrl = existing.ImageUrl;
        return View(movie);
    }

    // GET: Movies/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movie
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    // POST: Movies/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var movie = await _context.Movie.FindAsync(id);

        if (movie != null)
        {
            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();

            // delete the poster file too, so deleting a movie doesn't leave an orphaned file in storage
            await _blobService.DeleteFileAsync(movie.ImageUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool MovieExists(int? id)
    {
        return _context.Movie.Any(e => e.Id == id);
    }

    // Returns an error message if the file isn't an allowed image, otherwise null
    private static string? CheckImage(IFormFile file)
    {
        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            return "The poster must be a JPG, PNG or WebP image.";
        }

        if (file.Length > MaxImageBytes)
        {
            return "The poster must be 5 MB or smaller.";
        }

        return null;
    }

    // Unique file name (GUID) so uploading two files with the same name never overwrites one
    private static string NewFileName(IFormFile file)
    {
        return $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
    }
}
