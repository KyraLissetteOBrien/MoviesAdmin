using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Models;
using MoviesAdmin.Services;
using System.Reflection.Metadata.Ecma335;

public class MoviesController : Controller
{
    private readonly MoviesAdminContext _context;
    private readonly AzureStorageService _blobService; 

    public MoviesController(MoviesAdminContext context) 
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
        // check if the id parameter is null from the URL
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movie
            .FirstOrDefaultAsync(m => m.Id == id);

        // retrieve the movie with the specified id from the database
        var trail = await _context.Trail.FirstOrDefaultAsync(mbox => mbox.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    // to do - handle image being required 

    // GET: Movies/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Movies/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Title,Description,Runtime,Genres,Rating,Synopsis,Release,DateFunFact,Directors")] Movie movie, IFormFile imageFile)
    {
        if (ModelState.IsValid)
        {

            // save file in Azure storage
            string imageUrl = await _blobService.UploadFileAsync(imageFile, imageFile.FileName);

            // save image in Url movie record
            movie.ImageUrl = imageUrl;

            // save record in database 
            _context.Add(movie);
            await _context.SaveChangesAsync();

            // redirect to the index action after successfully creating the trail
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id,
        [Bind("Id,Title,Description,Genres,Rating,Runtime,Synopsis,ReleaseDate,FunFact,Directors")] Movie movie, IFormFile ImageFile) // - 
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

                throw;
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
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private bool MovieExists(int? id)
    {
        return _context.Movie.Any(e => e.Id == id);
    }
}

