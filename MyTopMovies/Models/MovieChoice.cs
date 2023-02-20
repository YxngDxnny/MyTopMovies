using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTopMovies.Models
{
    [Table("MovieChoices", Schema = "AppObj")]
    public class MovieChoice
    {
        [Key]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ChoiceID { get; set; }
        [Required]
        public string MovieID { get; set; }
        [Required]
        public string MovieName { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public int Rank { get; set; }
        [Required]
        public string ReleaseDate { get; set; }


        public string? SelectionID { get; set; }

        //Navigation Property
        public virtual MovieSelection Selection { get; set; }
    }
}
