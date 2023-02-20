using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyTopMovies.Areas.Identity.Data;
using MyTopMovies.Data;
using MyTopMovies.Models;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Claims;
using System.Xml.Linq;
using static MyTopMovies.Controllers.SelectionController;
using static MyTopMovies.Controllers.UserProfileController;

namespace MyTopMovies.Controllers
{
    ///<summary>Controller Class that manages and handle user Movie Lists</summary>
    public class ListsController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public ListsController(ApplicationContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        ///<summary>Creates Model and Displays an enumeration of all lists</summary>
        ///<param name="page">Page number of Movie List enumeration</param>
        ///<returns>The Lists Index View and passes a ListEnum type</returns>
        public IActionResult Index(int page=1)
        {
            IEnumerable<TopMovieList> topMovieLists = _db.TopMovieLists;
            List<ListDisplayModel> models = new List<ListDisplayModel>();

            foreach (var obj in topMovieLists)
            {
                ListDisplayModel model = new ListDisplayModel(obj, _db, _userManager.GetUserId(User));
                models.Add(model);
            }

            ListsEnum listsEnum = new ListsEnum(models, page, "Lists.Index", true);
            return View(listsEnum);
        }

        ///<summary>Displays Creation form</summary>
        ///<param name="returnPoint">string value representing Controller and action to return to after processing</param>
        ///<returns>View or redirects to returnpoint if the user is not logged in</returns>
        public IActionResult Create(string? returnPoint= null)
        {
            //Check if user is logged in
            bool isLoggedIn = (User?.Identity != null) && User?.Identity.IsAuthenticated == true;
            if (!isLoggedIn)
            {
                TempData["error"] = "You are not logged in";
                return RedirectToAction("Index");
            }

            if (returnPoint == "Home.Index")
                return RedirectToAction("Index", "Home");
            else
                return RedirectToAction("Index");
        }

        ///<summary>Validates & Creates a new Movie Lists with variables from List creation form</summary>
        ///<param name="obj">The new List object to be added</param>
        ///<returns>View or displays validation error on current page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TopMovieList obj)
        {
            var id = _userManager.GetUserId(User);
            obj.UserID = id;

            //Validate for duplicates
            var check = from l in _db.TopMovieLists where (l.ListName == obj.ListName) && (l.UserID==id) select l;
            if (check.Any())
            {
                ModelState.AddModelError("ListName", "You have already created a List with this exact name");
                return View();
            }

            //Validate Year filter inputs
            if (obj.YearFilter1==null && obj.YearFilter2!=null)
            {
                ModelState.AddModelError("YearFilter1", "To Select only one year, fill only the first input or select the same year in both inputs");
                return View();
            }

            if(obj.YearFilter1 != null && obj.YearFilter2 != null)
            if (obj.YearFilter1 > obj.YearFilter2 )
            {
                ModelState.AddModelError("YearFilter1", "'From' year cannot be later than 'To' year, pick an earlier year or the same year");
                return View();
            }

            _db.TopMovieLists.Add(obj);
            _db.SaveChanges();
            TempData["success"] = "List Created Succesfully";

            
            
            return RedirectToAction("Index");
        }

        ///<summary>Creates Overall Ranking Model and Displays an enumeration of all Choices in all complete selections</summary>
        ///<param name="listID">MovieList's ID to display overall ranking</param>
        ///<param name="selectionID">selection ID of users selection for this list</param>
        ///<param name="page">Page number of the View</param>
        ///<returns>An IActionResult that redirects to the appropraite returnPoint action</returns>
        public IActionResult OverallRanking(string listID, string? selectionID= null, int page=1)
        {
            TopMovieList list= _db.TopMovieLists.Find(listID);

            if (list == null)
            {
                TempData["error"] = "This List has been deleted";
                return RedirectToAction("Index", "Selection", new {selectionID= selectionID});
            }

            List<OverallChoice> choicesList = new List<OverallChoice>();
            var allSelections= from s in _db.MovieSelections where s.ListID==listID select s;
            int selectionCount = 0;

            foreach(var s in allSelections)
            {
                var allChoices = from c in _db.MovieChoices where c.SelectionID == s.SelectionID select c;
                if (allChoices.Count()<list.Count) continue;
                selectionCount++;

                foreach (var c in allChoices)
                {
                    var check = from ch in choicesList where ch.Choice.MovieID == c.MovieID select ch;

                    if(check.Any())
                    
                        check.First().Points += (float)list.Count / (float)c.Rank;
                    
                    else
                    {
                        var favEnum = from f in _db.FavouriteMovies where f.MovieID == c.MovieID && f.UserID == _userManager.GetUserId(User) select f;

                        choicesList.Add(new OverallChoice(c, favEnum.Any(), (float)list.Count / (float)c.Rank));
                    }
                }
                    
            }

            OverallEnum overallEnum = new OverallEnum(choicesList, list.Count, page, "Lists.OverallRanking", true);
            OverallRankingModel model = new OverallRankingModel(overallEnum, list, _db, _userManager.GetUserId(User), selectionCount);
            return View(model);
        }

        ///<summary>Removes Movie List from database</summary>
        ///<param name="listID">MovieList's ID to be removed</param>
        ///<param name="returnPoint">string value representing Controller and action to return to after processing</param>
        ///<param name="page">Page number of return page</param>
        ///<returns>An IActionResult that redirects to the appropraite returnPoint action</returns>
        public IActionResult Delete(string listID,string returnPoint,int page=1)
        {
            
            var list = _db.TopMovieLists.Find(listID);
            var listUserID = list.UserID;

            if (list != null)
            {
                //Delte all favourite entries for this Lists
                var favEnum = from f in _db.FavouriteLists where f.ListID == listID select f;

                if (favEnum.Any())
                    foreach (var f in favEnum)
                    {
                        var fav = _db.FavouriteLists.Find(f.ID);
                        _db.FavouriteLists.Remove(fav);
                    }

                //update Id's for selections from this list
                var selEnum = from s in _db.MovieSelections where s.ListID == listID select s;

                if (selEnum.Any())
                    foreach (var s in selEnum)
                    {
                        var sel = _db.MovieSelections.Find(s.SelectionID);
                        sel.ListID = null;
                        _db.MovieSelections.Update(sel);
                    }

                //Remove List entry from database
                _db.TopMovieLists.Remove(list);
                TempData["success"] = "List Deleted Sucessfully";
            }

            _db.SaveChanges();

            return ReturnTo(returnPoint,listUserID, page);
        }

        ///<summary>uses a Switch to to match return point strings to appropiate actions</summary>
        ///<param name="returnPoint">string value representing Controller and action to return to</param>
        ///<param name="userID">param needed by redirecting action</param>
        ///<param name="page">param needed by redirecting action</param>
        ///<returns>An IActionResult to the appropriate action</returns>
        IActionResult ReturnTo(string returnPoint, string? userID=null,int page=1)
        {
            switch (returnPoint)
            {
                case "Lists.Index": return RedirectToAction("Index", new { page = page });
                case "UserProfile.Index": return RedirectToAction("Index", "UserProfile", new { profileUserID = userID, page = page });
                case "UserProfile.UserLists": return RedirectToAction("UserLists", "UserProfile", new { profileUserID = userID, page = page });
                default: return RedirectToAction("Index", new { page = page });
            }
        }

        ///<summary>Searches all list for those whose name matches the search string queried</summary>
        ///<param name="page">page number of current search</param>
        ///<param name="searchString">Search string to query against Movie Lists table</param>
        ///<returns>The Lists Index View and passes a ListEnum type</returns>
        public IActionResult Search(string searchString, int page =1)
        {
            var lists = _db.TopMovieLists.FromSqlRaw($"SELECT * FROM AppObj.MovieLists where ListName Like '%" +searchString +"%'")
            .ToList();

            List<ListDisplayModel> models = new List<ListDisplayModel>();

            foreach(var l in lists)
            {
                models.Add(new ListDisplayModel(l, _db, _userManager.GetUserId(User)));
            }

            TempData["searchString"] = searchString;
            return View("~/Views/Lists/Index.cshtml", new ListsEnum(models, page, "Lists.Search", true));
        }

        ///<summary>Class for storing & displaying Overall Ranking's for Lists</summary>
        public class OverallRankingModel
        {
            public OverallEnum RankingEnum { get; set; }
            public TopMovieList List { get; set; }
            public bool HasSelection { get; set; }
            public int SelectionCount { get; set; }
            public OverallRankingModel(OverallEnum rankingEnum, TopMovieList list, ApplicationContext db, string userID, int selectionCount)
            {
                List<OverallChoice> orderedRankings = new List<OverallChoice>();
                var ol = from r in rankingEnum.Models orderby r.Points descending select r;
                int i = 1;
                foreach(var r in ol)
                {
                    if (i > list.Count) break;
                    orderedRankings.Add(r);
                    i++;
                }

                rankingEnum.Models= orderedRankings;
                RankingEnum = rankingEnum;
                List = list;
                SelectionCount = selectionCount;


                var check = from s in db.MovieSelections where s.ListID == list.ListID && s.UserID == userID select s;

                if (check.Any())
                    HasSelection = true;
                else HasSelection = false;
            }

            ///<summary>round up a double input to a specified number of places</summary>
            ///<param name="input">input to round up</param>
            ///<param name="places">number of places to round up input</param>
            ///<returns>a double rounded up to specified places </returns>
            public static double RoundUp(double input, int places)
            {
                double multiplier = Math.Pow(10, Convert.ToDouble(places));
                return Math.Ceiling(input * multiplier) / multiplier;
            }
        }

        ///<summary>Class for displaying enumeration of choices in the Overall Ranking for Lists</summary>
        public class OverallEnum: DisplayEnum
        {
            public List<OverallChoice> Models = new List<OverallChoice>();
            public int Page { get; set; } = 1;
            public string ReturnPoint { get; set; }
            public bool DisplayComplete { get; set; } = true;
            public int Count { get; set; }
            public OverallEnum(List<OverallChoice> models, int count, int page, string returnPoint, bool displayComplete = false) : base(models, page, returnPoint, displayComplete)
            {
                Models = models;
                DisplayComplete = displayComplete;
                Page = page;
                ReturnPoint = returnPoint;
                Count = count;
            }
        }

        ///<summary>Class for storing individual choices in the Overall Ranking for Lists</summary>
        public class OverallChoice:SelectionController.ChoiceModel
        {
            public MovieChoice Choice { get; set; }
            public bool IsFavourite { get; set; }
            public double Points { get; set; }
            public OverallChoice(MovieChoice choice, bool isFavourite, double points):base(choice, isFavourite)
            {
                Choice = choice;
                IsFavourite = isFavourite;
                Points = points;
            }
        }

        ///<summary>Class for storing & displaying Lists</summary>
        public class ListDisplayModel
        {
            public string ListID { get; set; }
            public string Name { get; set; }
            public string Creator { get; set; }
            public string CreatorID { get; set; }
            public bool IsFavourite { get; set; }
            public bool HasSelection { get; set; }
            public bool IsCreator { get; set; }
            public ListDisplayModel(TopMovieList list, ApplicationContext db, string userID)
            {
                ListID = list.ListID;
                Name = list.ListName;
                Creator = db.Users.Find(list.UserID).FullName;
                CreatorID = list.UserID;
                if (CreatorID == userID)
                    IsCreator = true;
                else
                    IsCreator = false;
                var favEnum = from f in db.FavouriteLists where f.ListID == ListID && f.UserID == userID select f;
                if(favEnum.Any())
                    IsFavourite= true;
                else
                    IsFavourite= false;

                var selEnum = from s in db.MovieSelections where s.ListID == ListID && s.UserID == userID select s;

                if (selEnum.Any())
                    HasSelection = true;
                else
                    HasSelection = false;
            }
        }

        ///<summary>Class for displaying an enumeration of Lists</summary>
        public class ListsEnum:DisplayEnum
        {
            public List<ListDisplayModel> Models { get; set; } = new List<ListDisplayModel>();
            public int Page { get; set; } = 1;
            public string ReturnPoint { get; set; }
            public bool DisplayComplete { get; set; } = true;
            public ListsEnum(List<ListDisplayModel> models, int page, string returnPoint, bool displayComplete = false):base(models, page, returnPoint, displayComplete)
            {
                Models = models;
                DisplayComplete = displayComplete;
                Page = page;
                ReturnPoint = returnPoint;
            }

        }


    }
}
