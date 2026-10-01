//Sprint 1 is what this is working towards
//Notes: movies must be sorted by release date, build according to the specifications set

using System.ComponentModel.DataAnnotations;

namespace MoviesAdmin.Models
{
    public class Movie
     
    {
        //1
        public int Id { get; set; }

        //2
        public string Title { get; set; } = string.Empty;

        //2
        public string Description { get; set; } = string.Empty;

        //3
        [Display(Name="Runtime (in Hours)")]
        [Range(1, 857)] //as per the longest movie listed in Wikepedia
        [Required]
        public int Runtime { get; set; }

        //4
        public string Rating { get; set; } = string.Empty;

        //5
        [Display(Prompt = "Enter as follows: Comedy, Horror, Documentary")]
        [Required]
        public string Genres { get; set; } = string.Empty; //so far this prompt works as a full website, but crunched down it gets cut off (pay mind to responsitivity in later development

        //6
        public string Synopsis { get; set; } = string.Empty;
    }
}
