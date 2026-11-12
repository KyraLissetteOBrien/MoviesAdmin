using System.ComponentModel.DataAnnotations;

namespace MoviesAdmin.Models
{
    public class UserSnapshot
    {
        [Key]
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
