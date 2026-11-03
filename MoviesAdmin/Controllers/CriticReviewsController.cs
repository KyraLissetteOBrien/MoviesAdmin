using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesAdmin;
using MoviesAdmin.Models;

// Only users in the "critic" role (stored in Auth0) can use this controller.
// Every query below is filtered by CreatedBy == _userId so a critic only ever sees their own reviews.
[Authorize(Roles = "critic")]
public class CriticReviewsController : Controller
{
    private readonly MoviesAdminContext _context;

    // The logged in user (Auth0 user id)
    private string _userId = string.Empty;

    // Constructor
    public CriticReviewsController(MoviesAdminContext context)
    {
        _context = context;
    }

    // The User object is not available when the constructor is called.
    // OnActionExecuting() runs right before the action method, so HttpContext and User are populated.
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        _userId = User.GetUserId();
    }

    // GET: CriticReviews
    // Only the logged in critic's reviews (drafts and published)
    public async Task<IActionResult> Index()
    {
        var reviews = await _context.CriticReview
            .Where(r => r.CreatedBy == _userId)
            .Include(r => r.Movie)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

        return View(reviews);
    }

    // GET: CriticReviews/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var criticReview = await _context.CriticReview
            .Where(r => r.CreatedBy == _userId)
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (criticReview == null)
        {
            return NotFound();
        }

        return View(criticReview);
    }

    // GET: CriticReviews/Create?movieId=5
    public async Task<IActionResult> Create(int? movieId)
    {
        // One review per movie: if this critic already reviewed the movie, go to their review instead
        if (movieId != null)
        {
            var existing = await _context.CriticReview
                .FirstOrDefaultAsync(r => r.MovieId == movieId && r.CreatedBy == _userId);

            if (existing != null)
            {
                return RedirectToAction(nameof(Details), new { id = existing.Id });
            }
        }

        await FillMovieListAsync(movieId);

        return View();
    }

    // POST: CriticReviews/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,MovieId,Description,Rating,IsPublished")] CriticReview criticReview)
    {
        // Assign the logged in user to the record (not typed in by the user)
        criticReview.CreatedBy = _userId;

        // The movie must exist
        if (!await _context.Movie.AnyAsync(m => m.Id == criticReview.MovieId))
        {
            ModelState.AddModelError("MovieId", "Please choose a movie.");
        }
        // One review per movie: a critic can not review the same movie twice
        else if (await _context.CriticReview.AnyAsync(r => r.MovieId == criticReview.MovieId && r.CreatedBy == _userId))
        {
            ModelState.AddModelError("MovieId", "You have already reviewed this movie.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(criticReview);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        await FillMovieListAsync(criticReview.MovieId);

        return View(criticReview);
    }

    // GET: CriticReviews/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Only the critic's own review, and only while it is still a draft
        var criticReview = await _context.CriticReview
            .Where(r => r.CreatedBy == _userId && r.IsPublished == false)
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (criticReview == null)
        {
            return NotFound();
        }

        return View(criticReview);
    }

    // POST: CriticReviews/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Description,Rating,IsPublished")] CriticReview criticReview)
    {
        if (id != criticReview.Id)
        {
            return NotFound();
        }

        // Load the saved review from the database: it must be the critic's own and still a draft.
        // (Checked again here on the POST so a published review can't be changed by sending the form directly.)
        var savedReview = await _context.CriticReview
            .Where(r => r.CreatedBy == _userId && r.IsPublished == false)
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (savedReview == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // Only these fields can change. The movie, created by and created date stay as saved.
            savedReview.Description = criticReview.Description;
            savedReview.Rating = criticReview.Rating;
            savedReview.IsPublished = criticReview.IsPublished; // ticking "Publish" locks the review

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Validation failed: show the form again with what the user typed
        criticReview.Movie = savedReview.Movie;
        criticReview.MovieId = savedReview.MovieId;

        return View(criticReview);
    }

    // GET: CriticReviews/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Only the critic's own review, and only while it is still a draft
        var criticReview = await _context.CriticReview
            .Where(r => r.CreatedBy == _userId && r.IsPublished == false)
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (criticReview == null)
        {
            return NotFound();
        }

        return View(criticReview);
    }

    // POST: CriticReviews/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        // Same rule on the POST: own review, draft only
        var criticReview = await _context.CriticReview
            .FirstOrDefaultAsync(r => r.Id == id && r.CreatedBy == _userId && r.IsPublished == false);

        if (criticReview == null)
        {
            return NotFound();
        }

        _context.CriticReview.Remove(criticReview);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // Fill the movie dropdown with the movies this critic has NOT reviewed yet (one review per movie)
    private async Task FillMovieListAsync(int? selectedMovieId)
    {
        var reviewedMovieIds = _context.CriticReview
            .Where(r => r.CreatedBy == _userId)
            .Select(r => r.MovieId);

        var movies = await _context.Movie
            .Where(m => !reviewedMovieIds.Contains(m.Id))
            .OrderBy(m => m.Title)
            .ToListAsync();

        ViewBag.MovieId = new SelectList(movies, "Id", "Title", selectedMovieId);
    }
}