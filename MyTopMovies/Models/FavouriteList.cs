using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MyTopMovies.Areas.Identity.Data;
using System.Diagnostics.CodeAnalysis;

namespace MyTopMovies.Models
{
    [Table("FavouriteList", Schema = "AppObj")]
    public class FavouriteList
    {
        [Key]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ID { get; set; }
        public string? ListID { get; set; }
        public string? UserID { get; set; }

        //Navigation Property
        public virtual TopMovieList MovieList { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
