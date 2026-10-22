using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MoviesAdmin.Models
{
    public class AudienceRating
    {
        public int Id { get; set; }

        [ValidateNever]
        public List<AudienceRating> AudienceRatings { get; set; } = new List<AudienceRating>();

        [Display(Prompt = "For example: PG-13, R")]
        public string Title { get; set; } = string.Empty;
    }
}
