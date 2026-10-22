using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Models;
using MoviesAdmin.Services;

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
        // Retrieve all movies from the database, sorted by release date (sprint requirement)
        List<Movie> movies = await _context.Movie.OrderBy(m => m.ReleaseDate).ToListAsync();

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
        var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);

        // Check if the movie exists in the database
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,Description,Runtime,Genres,Rating,Synopsis,ReleaseDate,FunFact,Directors")] Movie movie, IFormFile? imageFile) // collect inputs and file input from form
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
    // ImageUrl and CreatedDate come back from hidden fields in the form, so editing the title keeps them.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Title,Description,Genres,Rating,Runtime,Synopsis,ReleaseDate,FunFact,Directors,ImageUrl,CreatedDate")] Movie movie)
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
        return View(movie);
    }

    // GET: Movies/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);
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

            // delete the image file from Azure Storage too
            await _blobService.DeleteFileAsync(movie.ImageUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool MovieExists(int? id)
    {
        return _context.Movie.Any(e => e.Id == id);
    }
}
