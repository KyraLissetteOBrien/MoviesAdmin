using System.ComponentModel.DataAnnotations;

namespace MoviesAdmin.Models
{
    public class AudienceRating
    {
        public int Id { get; set; }

        [Display(Prompt = "For example: PG-13, R")]
        public string Title { get; set; } = string.Empty;
    }
}
