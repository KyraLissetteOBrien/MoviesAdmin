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
            try
            {
                _context.Add(criticReview);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // The unique index refused a second review of the same movie (for say a double-click).
                // If the critic's review is already saved, just show their list. Otherwise it is a different problem.
                bool alreadySaved = await _context.CriticReview
                    .AnyAsync(r => r.MovieId == criticReview.MovieId && r.CreatedBy == _userId);

                if (alreadySaved)
                {
                    return RedirectToAction(nameof(Index));
                }

                throw;
            }
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
    public async Task<IActionResult> Edit(int? id, [Bind("Id,MovieId,Description,Rating,IsPublished,CreatedDate")] CriticReview criticReview)
    {
        if (id != criticReview.Id)
        {
            return NotFound();
        }

        // Only allow the save if this review is the logged in critic's AND still a draft.
        // (A published review can no longer be modified.)
        bool isMyDraft = await _context.CriticReview
            .AnyAsync(r => r.Id == id && r.CreatedBy == _userId && r.IsPublished == false);

        if (!isMyDraft)
        {
            return NotFound();
        }

        // Assign userId to record (not typed in by the user)
        criticReview.CreatedBy = _userId;

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(criticReview);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CriticReviewExists(criticReview.Id))
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

        // Validation failed: load the movie again so the form can show its title
        criticReview.Movie = (await _context.Movie.FindAsync(criticReview.MovieId))!;

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

    private bool CriticReviewExists(int id)
    {
        return _context.CriticReview.Any(e => e.Id == id);
    }

    // Fill the movie dropdown with the movies this critic has ~not~ reviewed yet (one review per movie)
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