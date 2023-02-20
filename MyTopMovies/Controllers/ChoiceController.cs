using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTopMovies.Areas.Identity.Data;
using MyTopMovies.Data;
using MyTopMovies.Migrations;
using MyTopMovies.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.Xml;
using System.Text;
using static MyTopMovies.Controllers.ChoiceController;

namespace MyTopMovies.Controllers
{
    ///<summary>Class that manages and handles individual movie choices</summary>
    public class ChoiceController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public ChoiceController(ApplicationContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        ///<summary>Adds a new Choice to the database</summary>
        ///<param name="movieID">TMDB Movie ID</param>
        ///<param name="movieName">Movie Title</param>
        ///<param name="selectionID">Selection to Add choice to</param>
        ///<param name="releaseDate">Movie's release date</param>
        ///<param name="rank">Movie's intended rank in the selection</param>
        ///<param name="returnPoint">string value representing Controller and action to return to after processing</param>
        ///<param name="page">Page number of return page</param>
        ///<returns>An IActionResult that redirects to the appropraite action</returns>
        public IActionResult Add(string movieID, string movieName, string selectionID, string releaseDate, int rank, string returnPoint, int page=1)
        {
            #region Check if choice already exists
            var check = from c in _db.MovieChoices where (c.SelectionID == selectionID) && (c.MovieID == movieID) select c;

            if (check.Any())
            {
                TempData["error"] = "You have already added this movie to this selection";

                return ReturnTo(returnPoint, null, page, selectionID, rank);
            }
            #endregion

            #region Declare variables needed for Genre and Year filter validations
            Movie movie = MoviesController.GetMovie(Int32.Parse(movieID));
            MovieSelection selection = _db.MovieSelections.Find(selectionID);
            TopMovieList list= _db.TopMovieLists.Find(selection.ListID);
            #endregion

            #region Validate genre filter
            bool isSameGenre;

            if (list.GenreID == null)
                isSameGenre = true;
            else
                isSameGenre = IsSameGenre(movie.genres, (int)list.GenreID);

            if(!isSameGenre)
            {
                TempData["error"] = "This Movie is not of the same genre specified by the list";

                return ReturnTo(returnPoint, list.ListID, page, selectionID, rank);
            }
            #endregion

            #region Validate year range filter
            bool isWithinYearRange = IsWithinYearRange(GetYear(releaseDate), list.YearFilter1, list.YearFilter2);

            if (!isWithinYearRange)
            {
                if(list.YearFilter2==null) TempData["error"] = "This Movie is not within the year " +list.YearFilter1;
                else  TempData["error"] = "This Movie is not within the year range " + list.YearFilter1 + "-" + list.YearFilter2;

                return ReturnTo(returnPoint, list.ListID, page, selectionID, rank);
            }
            #endregion

            #region Initialise choice to be added
            MovieChoice movieChoice = new MovieChoice();
            movieChoice.MovieID = movieID;
            movieChoice.MovieName = movieName;
            movieChoice.SelectionID = selectionID;
            movieChoice.Rank = rank;
            movieChoice.ReleaseDate= releaseDate;
            movieChoice.Year = GetYear(releaseDate);
            #endregion

            #region check for and delete any choice with the same rank in the selection
            var oldRecord = from a in _db.MovieChoices where a.SelectionID==selectionID && a.Rank==rank select a;

            if(oldRecord.Any()) 
            {
                _db.MovieChoices.Remove(oldRecord.First());
            }
            #endregion

            #region savechanges and return
            _db.MovieChoices.Add(movieChoice);
            _db.SaveChanges();

            return ReturnTo(returnPoint, list.ListID, page, selectionID, rank);
            #endregion
        }

        ///<summary>Validates if any of the Genre element of the List has ID matches genreID</summary>
        ///<param name="genres">List of Genre Elements to check</param>
        ///<param name="genreID">specified ID to check for a match</param>
        ///<returns>true if at least one element matches the specified ID</returns>
        public bool IsSameGenre(List<Genre> genres, int genreID)
        {
            bool isSameGenre = false;

            foreach (var g in genres)
            {
                if (g.id == genreID)
                {
                    isSameGenre = true;
                    break;
                }
            }

            return isSameGenre;
        }


        ///<summary>Validates if the specified year is within the Year Ranges startYear to endYear</summary>
        ///<param name="year">specified year</param>
        ///<param name="startYear">minimum Year range</param>
        ///<param name="endYear">maximun Year range</param>
        ///<returns>true if year is within range or startYear is null</returns>
        public bool IsWithinYearRange(int year, int? startYear, int? endYear)
        {
            bool isWithinRange = false;

            if (startYear == null) isWithinRange = true; //No Year Filter applied to List
            else
            {
                if (endYear == null)
                {
                    if (year == startYear) isWithinRange = true;
                    else isWithinRange = false;
                }
                else
                {
                    if (endYear == startYear)
                        if (year == startYear) isWithinRange = true;
                        else
                        if (!(year >= startYear) || !(year <= endYear)) isWithinRange = false;
                }
            }
            return isWithinRange;
        }

        ///<summary>uses a Switch to to match return point strings to appropiate actions</summary>
        ///<param name="returnPoint">string value representing Controller and action to return to</param>
        ///<param name="listID">param needed by redirecting action</param>
        ///<param name="page">param needed by redirecting action</param>
        ///<param name="selectionID">param needed by redirecting action</param>
        ///<param name="rank">param needed by redirecting action</param>
        ///<returns>An IActionResult to the appropriate action</returns>
        public IActionResult ReturnTo(string returnPoint, string? listID = null, int page = 1, string? selectionID = null, int? rank = null)
        {
            switch (returnPoint)
            {
                case "Selection.Index": return RedirectToAction("Index", "Selection", new { listID = listID, page = page });
                case "Selection.SelectionIndex": return RedirectToAction("SelectionIndex", "Selection", new { selectionID = selectionID, page = page });
                case "Movies.Search": return RedirectToAction("Search", "Movies", new { selectionID = selectionID, rank = rank, page= page });
                case "Favourites.Movies": return RedirectToAction("Movies", "Favourites", new { selectionID = selectionID, rank = rank });
                default: return RedirectToAction("Index", "Selection", new { listID = listID, page = page });
            }
        }

        ///<summary>Removes a choice from a selection and delete it from the database</summary>
        ///<param name="returnPoint">string value representing Controller and action to return to after process</param>
        ///<param name="page">page of the return point to return to</param>
        ///<param name="choiceID">ID of the choice to be deleted</param>
        ///<returns>An IActionResult to the appropriate action</returns>
        public IActionResult Delete(string choiceID, string? returnPoint=null, int page=1)
        {
            var choice = _db.MovieChoices.Find(choiceID);
            int rank = choice.Rank;
            var selectionID= choice.SelectionID;
            var listID= _db.MovieSelections.Find(choice.SelectionID).ListID;

            if (choice != null)
            {
                _db.MovieChoices.Remove(choice);
            }

            _db.SaveChanges();
            return ReturnTo(returnPoint, listID, page, selectionID, rank);
        }

        ///<summary>Gets year from release date i.e yyyy-mm-dd</summary>
        ///<param name="releaseDate">Movie's release date</param>
        ///<returns>An integer representing the year only</returns>
        public static int GetYear(string releaseDate)
        {
            var year = new StringBuilder();

            foreach (char c in releaseDate)
            {
                if (c == ' ') continue;
                if (c == '-') break;

                year.Append(c);
            }

            return Int32.Parse(year.ToString());
        }
    }
}
