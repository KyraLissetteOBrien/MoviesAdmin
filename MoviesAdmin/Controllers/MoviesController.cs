using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesAdmin;
using MoviesAdmin.Models;
using MoviesAdmin.Services;

[Authorize]
public class MoviesController : Controller
{
    private readonly MoviesAdminContext _context;
    private readonly AzureStorageService _blobService;

    // Constructor - the framework "injects" the database context and the upload service (Dependency Injection)
    public MoviesController(MoviesAdminContext context, AzureStorageService blobService)
    {
        _context = context;
        _blobService = blobService;
    }

    // GET: Movies
    public async Task<IActionResult> Index()
    {
        //Remove later - testing
        var user = User.GetUserId();
        if (user != null)
            Console.WriteLine("Logged in user: " + user);

        // Retrieve all movies from the database, sorted by release date (sprint requirement)
        // Include loads the related Genre and AudienceRating rows so their names can be shown
        List<Movie> movies = await _context.Movie
            .Include(m => m.Genre)
            .Include(m => m.AudienceRating)
            .OrderBy(m => m.ReleaseDate)
            .ToListAsync();

        return View(movies);
    }

    // GET: Movies/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        // Check if the id parameter is null from URL
        if (id == null)
        {
            return NotFound();
        }

        // Retrieve the movie with the specified id from the database
        var movie = await _context.Movie
            .Include(m => m.Genre)
            .Include(m => m.AudienceRating)
            .FirstOrDefaultAsync(m => m.Id == id);

        // Check if the movie exists in the database
        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    // GET: Movies/Create
    [Authorize(Roles = "Admin")] // the data is stored in auth0, in the cookie
    public IActionResult Create()
    {
        var audienceRatings = _context.AudienceRating.ToList();
        ViewBag.AudienceRatingId = new SelectList(audienceRatings, "Id", "Title");
        var genres = _context.Genre.ToList();
        ViewBag.GenreId = new SelectList(genres, "Id", "Title");
        return View();
    }

    // POST: Movies/Create
    [Authorize(Roles = "Admin")] // the data is stored in auth0, in the cookie
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,Description,Runtime,GenreId,AudienceRatingId,Synopsis,ReleaseDate,FunFact,Directors,ImageUrl")] Movie movie, IFormFile? imageFile) // collect inputs and file input from form
    {
        // the image is required: give the user feedback if it is missing
        if (imageFile == null || imageFile.Length == 0)
        {
            ModelState.AddModelError("imageFile", "Please choose a poster image.");
        }

        // ImageUrl is filled in below, it is not typed into the form
        ModelState.Remove(nameof(Movie.ImageUrl));

        // Check if the model state is valid before adding the movie to the database
        if (ModelState.IsValid)
        {
            // Save file in Azure Storage using a unique GUID file name so files don't get overwritten
            string fileName = Guid.NewGuid() + Path.GetExtension(imageFile!.FileName);
            string imageUrl = await _blobService.UploadFileAsync(imageFile, fileName);

            // Save image Url in Movie record
            movie.ImageUrl = imageUrl;

            // Save record in database
            _context.Add(movie);
            await _context.SaveChangesAsync();

            // Redirect to the Index action after successfully creating the movie
            return RedirectToAction(nameof(Index));
        }

        // Validation failed: refill the dropdowns and keep the selected values
        ViewBag.AudienceRatingId = new SelectList(_context.AudienceRating.ToList(), "Id", "Title", movie.AudienceRatingId);
        ViewBag.GenreId = new SelectList(_context.Genre.ToList(), "Id", "Title", movie.GenreId);

        return View(movie);
    }

    // GET: Movies/Edit/5
    [Authorize(Roles = "Admin")]
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

        // Fill the dropdowns and pre-select the movie's current genre and rating
        ViewBag.AudienceRatingId = new SelectList(_context.AudienceRating.ToList(), "Id", "Title", movie.AudienceRatingId);
        ViewBag.GenreId = new SelectList(_context.Genre.ToList(), "Id", "Title", movie.GenreId);

        return View(movie);
    }

    // POST: Movies/Edit/5
    // ImageUrl and CreatedDate come back from hidden fields in the form, so editing the title keeps them.
    [Authorize(Roles = "Admin")] // the data is stored in auth0, in the cookie
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Title,Description,Runtime,GenreId,AudienceRatingId,Synopsis,ReleaseDate,FunFact,Directors,ImageUrl,CreatedDate")] Movie movie)
    {
        if (id != movie.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
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
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // Validation failed: refill the dropdowns and keep the selected values
        ViewBag.AudienceRatingId = new SelectList(_context.AudienceRating.ToList(), "Id", "Title", movie.AudienceRatingId);
        ViewBag.GenreId = new SelectList(_context.Genre.ToList(), "Id", "Title", movie.GenreId);

        return View(movie);
    }

    // GET: Movies/Delete/5
    [Authorize(Roles = "Admin")] // the data is stored in auth0, in the cookie
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movie
            .Include(m => m.Genre)
            .Include(m => m.AudienceRating)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    // POST: Movies/Delete/5
    [Authorize(Roles = "Admin")] // the data is stored in auth0, in the cookie
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var movie = await _context.Movie.FindAsync(id);
        if (movie != null)
        {
            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();

            // Delete the image file from Azure Storage too
            await _blobService.DeleteFileAsync(movie.ImageUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool MovieExists(int? id)
    {
        return _context.Movie.Any(e => e.Id == id);
    }

    public IActionResult DebugClaims()
    {
        var claims = string.Join("<br>",
            User.Claims.Select(c => $"{c.Type}: {c.Value}"));

        return Content(claims, "text/html");
    }

    public IActionResult TestRole()
    {
        return Content(User.IsInRole("Admin").ToString());
    }
}