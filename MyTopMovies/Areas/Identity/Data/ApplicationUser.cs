using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MyTopMovies.Models;

namespace MyTopMovies.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public byte[]? Picture { get; set; }

    //Navigation Property
    [ForeignKey("UserID")]
    public virtual ICollection<TopMovieList> Lists { get; set; }

    [ForeignKey("UserID")]
    public virtual ICollection<MovieSelection> Selections { get; set; }
    [ForeignKey("UserID")]
    public virtual ICollection<FavouriteList> Favourites { get; set; }
    [ForeignKey("UserID")]
    public virtual ICollection<FavouriteMovie> FavouriteMovies { get; set; }

}

