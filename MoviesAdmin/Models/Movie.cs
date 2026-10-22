//Sprint 1
//Notes: movies must be sorted by release date, build according to the specifications set

using System.ComponentModel.DataAnnotations;

namespace MoviesAdmin.Models
{
    public class Movie

    {
        //1
        public int Id { get; set; }

        //2
        [StringLength(179)] // based on the Guinness World Record for the longest film title, which the english version of, is called Revenge. 
        [Required]
        public string Title { get; set; } = string.Empty;

        //2 - Found via https://www.imdb.com/
        [StringLength(4266)] // based on the longest Wikepedia summary currently, for Alley Cats Strike 
        public string Description { get; set; } = string.Empty;

        //3
        [Display(Name = "Runtime (in Hours)")]
        [Range(1, 857)] //as per the longest movie listed in Wikepedia
        [Required]
        public int Runtime { get; set; }

        //4
        [Display(Name = "Viewer Classification", Prompt = "For example: Rated E, PG-13, R")]
        public string Rating { get; set; } = string.Empty;

        //5
        [Display(Prompt = "Enter as follows: Comedy, Horror, Documentary")]
        [Required]
        public string Genres { get; set; } = string.Empty;
        //so far this ^ prompt works as a full website, but crunched down it gets cut off (pay mind to responsitivity in later development

        //6 
        [StringLength(4266)]
        public string Synopsis { get; set; } = string.Empty;

        //can later add a box for trivia/a fun fact about the film 

        //7
        [Display(Name = "Release Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy}", ApplyFormatInEditMode = false)]
        public DateTime ReleaseDate { get; set; }

        //8 - fun fact (ensure its linked and added)
        [Display(Name = "Fun Fact", Prompt = "Like: This movie was filmed using mic'd bees")]
        public string FunFact { get; set; } = string.Empty;

        //9 - director(s) (ensure its linked and added)
        [Display(Prompt = "Enter at least 1")]
        public string Directors { get; set; } = string.Empty;

        //10 - URL to make the image store in Azure 
        public string ImageUrl { get; set; } = string.Empty;

        //11 - Time stamping for when the entry was saved 
        [DisplayFormat(DataFormatString = "{0:MMMM d, yyyy}")]
        [Display(Name = "Created")]
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now; // default to current time using the built in functionality 
    }
}
