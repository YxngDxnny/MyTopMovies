using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using MyTopMovies.Areas.Identity.Data;

namespace MyTopMovies.Models
{
    [Table("MovieSelections", Schema = "AppObj")]
    public class MovieSelection
    {
    
        [Key]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SelectionID { get; set; }
        public string? ListID { get; set; }
        [Required]
        public string? ListName { get; set; }
        [Required]
        public int Count { get; set; }
        public string? UserID { get; set; }

        //Navigation Property
        public virtual TopMovieList MovieList { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<MovieChoice> Choices { get; set; }


    }
}
