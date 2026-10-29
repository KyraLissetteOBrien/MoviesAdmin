//using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace MoviesAdmin.Models
//{
//    public class CriticReview
//    {
//        //1 db: Primary key
//        public int Id { get; set; }

//        // db: Foreign keys
//        [ForeignKey(nameof(Genre))]
//        public int GenreId { get; set; }

//        [ForeignKey(nameof(AudienceRating))]
//        public int AudienceRatingId { get; set; }

//        [ValidateNever]
//        public Genre Genre { get; set; } = null!; // Navigation property

//        [ValidateNever]
//        public AudienceRating AudienceRating { get; set; } = null!; // Navigation property

//        [Required]
//        public string Description { get; set; } = string.Empty;

//        //[Range(1, 5)]
//        //public int Rating { get; set; }

//        [Required]
//        public bool IsPublished { get; set; }

//        public string CreatedBy { get; set; } = string.Empty;

//        [ScaffoldColumn(false)]
//        public DateTime CreatedDate { get; set; } = DateTime.Now;
//    }
//}
