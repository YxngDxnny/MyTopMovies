using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MyTopMovies.Areas.Identity.Data;
using System.Diagnostics.CodeAnalysis;

namespace MyTopMovies.Models
{
    [Table("FavouriteMovie", Schema = "AppObj")]
    public class FavouriteMovie
    {
        [Key]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ID { get; set; }
        public string MovieID { get; set; }
        public string MovieName { get; set; }
        public int Year { get; set; }
        public string ReleaseDate { get; set; }
        public string? UserID { get; set; }

        //Navigation Property
        public virtual ApplicationUser User { get; set; }
    }
}
