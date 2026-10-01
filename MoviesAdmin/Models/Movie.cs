//Sprint 1 is what this is working towards
//Notes: movies must be sorted by release date, build according to the specifications set

namespace MoviesAdmin.Models
{
    public class Movie
     
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Runtime { get; set; }

        public int Rating { get; set; }

        public string Genres { get; set; } = string.Empty;

        public string Synopsis { get; set; } = string.Empty;
    }
}
