using Microsoft.EntityFrameworkCore;
using MoviesAdmin.Models;

public class MoviesAdminContext(DbContextOptions<MoviesAdminContext> options) : DbContext(options)
{
    public DbSet<MoviesAdmin.Models.Movie> Movie { get; set; } = default!;
    public DbSet<MoviesAdmin.Models.Genre> Genre { get; set; } = default!;
    public DbSet<MoviesAdmin.Models.AudienceRating> AudienceRating { get; set; } = default!;
    public DbSet<MoviesAdmin.Models.CriticReview> CriticReview { get; set; } = default!;
    public DbSet<UserSnapshot> UserSnapshot { get; set; } = default!;
}
