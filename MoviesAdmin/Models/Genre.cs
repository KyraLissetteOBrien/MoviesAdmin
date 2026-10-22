using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MoviesAdmin.Models
{
    public class Genre
    {
        public int Id { get; set; }

        [ValidateNever]
        public List<Genre> Genres { get; set; } = new List<Genre>();

        public string Title { get; set; } = string.Empty;
    }
}
