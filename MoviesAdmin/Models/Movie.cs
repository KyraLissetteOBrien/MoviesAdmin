using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesAdmin.Models
{
    public class Movie

    {
        //1 db: Primary key
        public int Id { get; set; }

        // db: Foreign keys
        [ForeignKey(nameof(Genre))]
        [Display(Name = "Genre")]
        [Required]
        public int GenreId { get; set; }

        [ForeignKey(nameof(AudienceRating))]
        [Display(Name = "Rating")]
        [Required]
        public int AudienceRatingId { get; set; }

        [ValidateNever]
        [ScaffoldColumn(false)]
        [Required]
        public Genre Genre { get; set; } = null!; // Navigation property

        //Based on https://en.wikipedia.org/wiki/Canadian_motion_picture_rating_systems 
        [ValidateNever]
        [ScaffoldColumn(false)]
        [Required]
        public AudienceRating AudienceRating { get; set; } = null!; // Navigation property

        [ValidateNever]
        [ScaffoldColumn(false)]
        [Required]
        public List<CriticReview> Reviews { get; set; } = new List<CriticReview>(); // Navigation property

        [StringLength(179, MinimumLength = 1)] // based on the Guinness World Record for the longest film title, which the english version of, is called Revenge. 
        [Required]
        public string Title { get; set; } = string.Empty;

        //Found via https://www.imdb.com/
        [StringLength(4266, MinimumLength = 7, ErrorMessage = "Description must be between 7 and 4266 characters.")] // based on the longest Wikepedia summary currently, for Alley Cats Strike
        [Required]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Runtime (in Hours)")]
        [Range(1, 857)] //as per the longest movie listed in Wikepedia
        [Required]
        public int Runtime { get; set; }

        [StringLength(4266, MinimumLength = 7, ErrorMessage = "Synopsis must be between 7 and 4266 characters.")]
        [Required]
        public string Synopsis { get; set; } = string.Empty;

        //The oldest recorded movie release date is 14 October 1888, when Louis Le Prince filmed "Roundhay Garden Scene." This short silent film is considered the oldest surviving film and features members of Le Prince's family playing in a garden, but because this uses datetime it will require more logic to limit
        [Display(Name = "Release Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy}", ApplyFormatInEditMode = false)]
        [Required]
        public DateTime? ReleaseDate { get; set; }

        //Fun fact 
        [Display(Name = "Fun Fact", Prompt = "Like: This movie was filmed using mic'd bees")]
        [StringLength(1000)]
        public string FunFact { get; set; } = string.Empty;

        //Director(s) 
        [Display(Prompt = "Enter at least 1")]
        [StringLength(2000)]
        [Required]
        public string Directors { get; set; } = string.Empty;

        //URL to make the image store in Azure 
        public string ImageUrl { get; set; } = string.Empty;

        //Time stamping for when the entry was saved 
        [DisplayFormat(DataFormatString = "{0:MMMM d, yyyy}")]
        [Display(Name = "Created")]
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now; // default to current time using the built in functionality 
    }
}
