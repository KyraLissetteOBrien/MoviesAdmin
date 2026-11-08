using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MoviesAdmin.Models
{
    [Index(nameof(MovieId), nameof(CreatedBy), IsUnique = true)] // One review per critic per movie
    public class CriticReview
    {
        // db: Primary key
        public int Id { get; set; }

        // db: Foreign key - the movie being reviewed
        [Display(Name = "Movie")]
        public int MovieId { get; set; }

        // Navigation property
        [ValidateNever]
        [ScaffoldColumn(false)]
        public Movie Movie { get; set; } = null!;

        // Review text, 1000 characters max 
        [Required]
        [StringLength(1000)]
        [Display(Name = "Review")]
        public string Description { get; set; } = string.Empty;

        // Numerical rating, 0 to 5 
        [Required]
        [Range(1, 5)]
        [Display(Name = "Rating (1-5)")]
        public int Rating { get; set; }

        [Display(Name = "Published")]
        public bool IsPublished { get; set; } = false;

        // Filled in by the controller (never typed into a form), so it is not validated.
        [StringLength(100)]
        [ValidateNever]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = string.Empty;

        [ScaffoldColumn(false)]
        [DisplayFormat(DataFormatString = "{0:MMMM d, yyyy}")]
        [Display(Name = "Created")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
