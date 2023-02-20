using MyTopMovies.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace MyTopMovies.Models
{
    [Table("MovieLists", Schema = "AppObj")]
    public class TopMovieList
    {

        [Key]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ListID { get; set; }

        [Required(ErrorMessage = "A name is required for this list")]
        [StringLength(100, MinimumLength = 10,
                  ErrorMessage = "Name should be between 10 and 100 characters")]
        [DisplayName("List Name")]
        public string ListName { get; set; }

        [Required(ErrorMessage = "Write a short description for this list")]
        [StringLength(250, MinimumLength = 15,
                  ErrorMessage = "Description should be between 20 and 250 characters")]
        public string Description { get; set; }

        [Required]
        [Range(3, 100, ErrorMessage = "Count must be between 3 to 100")]
        public int Count { get; set; }
        public string? UserID { get; set; }
        [DisplayName("Genre")]
        public int? GenreID { get; set; }
        [DisplayName("From")]
        [Range(1900, 2023, ErrorMessage = "Invalid year")]
        public int? YearFilter1 { get; set; }
        [DisplayName("To")]
        [Range(1900, 2023, ErrorMessage = "Invalid year")]
        public int? YearFilter2 { get; set; }

        //Navigation Property
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<MovieSelection> Selections { get; set; }
        public virtual ICollection<FavouriteList> Favourites { get; set; }
    }
}
