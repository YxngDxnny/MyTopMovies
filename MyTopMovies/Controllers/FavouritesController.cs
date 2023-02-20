using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyTopMovies.Areas.Identity.Data;
using MyTopMovies.Data;
using MyTopMovies.Models;
using static MyTopMovies.Controllers.ListsController;
using static MyTopMovies.Controllers.UserProfileController;

namespace MyTopMovies.Controllers
{
    ///<summary>Controller Class that manages and handle user Favourite Lists and Movies</summary>
    public class FavouritesController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public FavouritesController(ApplicationContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        ///<summary>Creates Model and Displays previews of favourite movies and lists</summary>
        ///<returns>The Favourite Index View and passes a FavouriteViewModel type</returns>
        public IActionResult Index()
        {
            //Check if User is Logged In
            bool isLoggedIn = (User?.Identity != null) && User?.Identity.IsAuthenticated == true;
            if (!isLoggedIn)
            {
                TempData["error"] = "You are not logged in";
                return RedirectToAction("Index");
            }

            //Get Favourite Movies for the current user
            int itemCount = 0;

            var favMoviesQ = from f in _db.FavouriteMovies where f.UserID == _userManager.GetUserId(User) select f;
            List<FavouriteMovie> favMovies = new List<FavouriteMovie>();

            foreach (var f in favMoviesQ)
            {
                favMovies.Add(f);
                itemCount++;
                if (itemCount > Display.PreviewItemCount) break;
            }

            //Get Favourite Lists for the current user
            itemCount = 0;

            var favListsQ = from f in _db.FavouriteLists where f.UserID == _userManager.GetUserId(User) select f;
            List<ListsController.ListDisplayModel> favLists = new List<ListsController.ListDisplayModel>();

            foreach (var f in favListsQ)
            {
                var list = _db.TopMovieLists.Find(f.ListID);
                favLists.Add(new ListsController.ListDisplayModel(list, _db, _userManager.GetUserId(User)));
                if (itemCount > Display.PreviewItemCount) break;
            }

            //Save and return View
            var listEnum = new ListsController.ListsEnum(favLists, 1, "Favourites.Index", false);
            var movieEnum= new FavMovieEnum(favMovies, 1, "Favourites.Index", false);
            FavouriteViewModel model = new FavouriteViewModel(movieEnum, listEnum);

            return View(model);
        }

        ///<summary>Creates Model and Displays full list of favourite movies for the current user</summary>
        ///<param name="selectionID">ID of the Selection to Add Favourite Movie as a choice to</param>
        ///<param name="rank">intended rank for the selection to Add Favourite Movie as a choice to</param>
        ///<param name="page">Page number of Favourite Movie List</param>
        ///<returns>The Favourite Movies View and passes a FavouriteMovieEnum type</returns>
        public IActionResult Movies(int page = 1, string? selectionID = null, int? rank = null)
        {
            var favEnum = from f in _db.FavouriteMovies where f.UserID == _userManager.GetUserId(User) select f;
            List<FavouriteMovie> favourites = new List<FavouriteMovie>();

            foreach (var f in favEnum)
            {
                favourites.Add(f);
            }

            return View(new FavMovieEnum(favourites, page, "Favourites.Movies", true, new SelectionController.AddToModel(selectionID, rank)));
        }

        ///<summary>Creates Model and Displays full list of favourite movies for the current user</summary>
        ///<param name="page">Page number of Favourite Lists List</param>
        ///<returns>The Favourite Lists View and passes a ListsController.ListsEnum type</returns>
        public IActionResult Lists(int page = 1)
        {
            var userID = _userManager.GetUserId(User);
            var favQ = from f in _db.FavouriteLists where f.UserID == userID select f;
            List<ListsController.ListDisplayModel> favourites = new List<ListsController.ListDisplayModel>();

            foreach (var f in favQ)
            {
                TopMovieList list = _db.TopMovieLists.Find(f.ListID);
                favourites.Add(new ListDisplayModel(list, _db, userID));
            }

            return View(new ListsController.ListsEnum(favourites, page, "Favourites.Lists", true));
        }

        ///<summary>Adds or Removes a Movie or Choice from favourites for a user</summary>
        ///<param name="movieID">TMDB Movie ID</param>
        ///<param name="movieName">Movie Title</param>
        ///<param name="releaseDate">Movie's release date</param>
        ///<param name="returnPoint">string value representing Controller and action to return to after processing</param>
        ///<param name="page">Page number of return page</param>
        ///<param name="selectionID">ID of the Selection to Add Favourite Movie as a choice to</param>
        ///<param name="rank">intended rank for the selection to Add Favourite Movie as a choice to</param>
        ///<param name="searchString">Search string for current Movie Search</param>
        ///<returns>An IActionResult that redirects to the appropraite returnPoint action</returns>
        public IActionResult ToggleFavouriteMovie(string movieID, string movieName, string releaseDate, string returnPoint, int page=1, string? selectionID = null, int? rank = null, String? searchString = null)
        {
            //Check if user is logged in
            bool isLoggedIn = (User?.Identity != null) && User?.Identity.IsAuthenticated == true;
            if (!isLoggedIn)
            {
                TempData["error"] = "You are not logged in";
                return RedirectToAction("Index");
            }

            //Check if favourite movie entry exists for the current user 
            string favID;
            string userID = _userManager.GetUserId(User);

            var favQ = from f in _db.FavouriteMovies where f.MovieID == movieID && f.UserID == userID select f;

            //Remove entry if it exists Add otherwise
            if (favQ.Any())
            {
                _db.FavouriteMovies.Remove(_db.FavouriteMovies.Find(favQ.First().ID));
                TempData["success"] = "Movie removed from favourites";
            }
            else
            {
                FavouriteMovie fav = new FavouriteMovie();
                fav.UserID = userID;
                fav.MovieID = movieID;
                fav.MovieName = movieName;
                fav.ReleaseDate = releaseDate;
                fav.Year = ChoiceController.GetYear(releaseDate);
                _db.FavouriteMovies.Add(fav);
                TempData["success"] = "Movie added to favourites";
            }

            //Save and redirect 
            _db.SaveChanges();

            return MoviesReturnTo(returnPoint, page, new SelectionController.AddToModel(selectionID, rank, searchString));      
        }

        ///<summary>Adds or Removes a MovieList from favourites for a user</summary>
        ///<param name="listID">MovieList's ID</param>
        ///<param name="returnPoint">string value representing Controller and action to return to after processing</param>
        ///<param name="page">Page number of return page</param>
        ///<returns>An IActionResult that redirects to the appropraite returnPoint action</returns>
        public IActionResult ToggleFavouriteList(string listID, string returnPoint, int page = 1)
        {
            bool isLoggedIn = (User?.Identity != null) && User?.Identity.IsAuthenticated == true;
            if (!isLoggedIn)
            {
                TempData["error"] = "You are not logged in";
                return RedirectToAction("Index");
            }

            string favID;
            string userID = _userManager.GetUserId(User);
            var favEnum = from f in _db.FavouriteLists where f.ListID == listID && f.UserID == userID select f;

            if (favEnum.Any())
            {
                favID = favEnum.First().ID;
                _db.FavouriteLists.Remove(_db.FavouriteLists.Find(favID));
                TempData["success"] = "List removed from favourites";
            }
            else
            {
                FavouriteList fav = new FavouriteList();
                fav.ListID = listID;
                fav.UserID = userID;
                _db.FavouriteLists.Add(fav);
                TempData["success"] = "List added to favourites";
            }

            //Save and return View
            _db.SaveChanges();
            TopMovieList list = _db.TopMovieLists.Find(listID);

            return ListsReturnTo(returnPoint, list.UserID, page);
        }

        ///<summary>uses a Switch to to match return point strings to appropiate actions</summary>
        ///<param name="returnPoint">string value representing Controller and action to return to</param>
        ///<param name="addTo">param needed by redirecting action</param>
        ///<param name="page">param needed by redirecting action</param>
        ///<returns>An IActionResult to the appropriate action</returns>
        IActionResult MoviesReturnTo(string returnPoint, int page=1,SelectionController.AddToModel addTo=null)
        {
            string? listID=null;
            var sel = _db.MovieSelections.Find(addTo.SelectionID);
            if (sel != null) listID = sel.ListID;
            switch(returnPoint)
            {
                case "Choice.SearchPost": return RedirectToAction("SearchPost", "Choice", new { searchString = addTo.SearchString, selectionID = addTo.SelectionID, rank= addTo.Rank }); ;
                case "Selection.SelectionIndex": return RedirectToAction("SelectionIndex", "Selection", new { selectionID = addTo.SelectionID, page=page});
                case "Selection.Index": return RedirectToAction("SelectionIndex", "Selection", new { selectionID = addTo.SelectionID, page = page });
                case "Selection.UserSelection": return RedirectToAction("UserSelection", "Selection", new { selectionID = addTo.SelectionID, page = page });
                case "Lists.OverallRanking": return RedirectToAction("OverallRanking", "Lists", new { listID = listID, selectionID = addTo.SelectionID, page = page });
                case "Favourites.Index": return RedirectToAction("Index");
                case "Favourites.Movies": return RedirectToAction("Lists", new { page = page, searchString = addTo.SearchString, selectionID = addTo.SelectionID, rank = addTo.Rank });
                default : return RedirectToAction("Index");
            }
        }

        ///<summary>uses a Switch to to match return point strings to appropiate actions</summary>
        ///<param name="returnPoint">string value representing Controller and action to return to</param>
        ///<param name="userID">param needed by redirecting action</param>
        ///<param name="page">param needed by redirecting action</param>
        ///<returns>An IActionResult to the appropriate action</returns>
        IActionResult ListsReturnTo(string returnPoint, string? userID = null, int page = 1)
        {
            switch (returnPoint)
            {
                case "Lists.Index": return RedirectToAction("Index", "Lists", new { page = page });
                case "UserProfile.Index": return RedirectToAction("Index", "UserProfile", new { profileUserID = userID, page = page });
                case "UserProfile.UserIndex": return RedirectToAction("UserIndex", "UserProfile", new { profileUserID = userID});
                case "UserProfile.UserLists": return RedirectToAction("UserLists", "UserProfile", new { profileUserID = userID, page = page });
                case "Favourites.Index": return RedirectToAction("Index");
                case "Favourites.Lists": return RedirectToAction("Lists", new { page = page });
                default: return RedirectToAction("Index");
            }
        }

        ///<summary>Class for displaying Favourite Movies and Lists</summary>
        public class FavouriteViewModel
        {
            public FavMovieEnum MovieEnum { get; set; }
            public ListsController.ListsEnum ListEnum { get; set; }
            public FavouriteViewModel(FavMovieEnum movieEnum, ListsController.ListsEnum listEnum)
            {
                MovieEnum = movieEnum;
                ListEnum = listEnum;
            }
        }

        ///<summary>Class for displaying Favourite Movies and Lists</summary>
        public class FavMovieEnum : DisplayEnum
        {
            public List<FavouriteMovie> Models { get; set; }= new List<FavouriteMovie>();
            public bool DisplayComplete { get; set; } = true;
            public int Page { get; set; } = 1;
            public string ReturnPoint { get; set; }
            public SelectionController.AddToModel AddTo { get; set; } = null;
            public FavMovieEnum(List<FavouriteMovie> models, int page, string returnPoint, bool displayComplete, SelectionController.AddToModel addTo = null): base(models, page, returnPoint, displayComplete)
            {
                Models = models;
                DisplayComplete = displayComplete;
                Page = page;
                ReturnPoint = returnPoint;
                AddTo = addTo;
            }
        }

    }

}
