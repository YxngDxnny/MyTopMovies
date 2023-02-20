using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTopMovies.Areas.Identity.Data;
using MyTopMovies.Data;
using MyTopMovies.Models;

namespace MyTopMovies.Controllers
{
    ///<summary>Controller Class that manages and handle user selections</summary>
    public class SelectionController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public SelectionController(ApplicationContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
       }

        ///<summary>Creates Model and Displays an enumeration of all choices in user selections for the list</summary>
        ///<param name="listID">MovieList's ID to display selection for</param>
        ///<param name="page">Page number of Selection enumeration</param>
        ///<returns>The Selection Index View and passes a SelectionChoicesEnum type</returns>
        public IActionResult Index(string listID, int page=1)
        {
            //Check if user is logged in
            bool isLoggedIn = (User?.Identity != null) && User?.Identity.IsAuthenticated == true;
            if (!isLoggedIn)
            {
                TempData["error"] = "You are not logged in";
                return RedirectToAction("Index", "Lists");
            }

            MovieSelection selection;
            var userID = _userManager.GetUserId(User);

            
            //search if selection exists for this user and selected list
            var selectionEnum = (from a in _db.MovieSelections where (a.ListID == listID && a.UserID == userID) select a);
            

            //Set Selection
            selection = selectionEnum.First();

            //Create SelectionModel & ViewData
            var selectionModel = new SelectionChoicesEnum(page, "Selection.Index", true, selection, _db, userID);
            ViewData["SelectionStatus"]= SetViewData(selectionModel);
            return View(selectionModel);
        }

        ///<summary>Creates Model and Displays an enumeration of all choices in user selections for the list</summary>
        ///<param name="selectionID">Movie Selection's ID to display</param>
        ///<param name="page">Page number of Selection enumeration</param>
        ///<returns>The Selection Index View and passes a SelectionChoicesEnum type</returns>
        public IActionResult SelectionIndex(string selectionID, int page=1)
        {
            var userID = _userManager.GetUserId(User);

            MovieSelection selection = _db.MovieSelections.Find(selectionID);

            if (userID != selection.UserID)
                return RedirectToAction("UserSelection", new { selectionID = selectionID });

                //Set SelectionModel & ViewData
                var selectionModel = new SelectionChoicesEnum(page, "Selection.SelectionIndex", true, selection, _db, userID);
            ViewData["SelectionStatus"] = SetViewData(selectionModel);
            return View("Index", selectionModel);
        }

        ///<summary>Creates Model and Displays an enumeration of all choices in another user's selections for the list</summary>
        ///<param name="selectionID">Movie Selection's ID to display</param>
        ///<param name="page">Page number of Selection enumeration</param>
        ///<returns>The Selection Index View and passes a SelectionChoicesEnum type</returns>
        public IActionResult UserSelection(string selectionID, int page=1)
        {

            MovieSelection selection;
            var userID = _userManager.GetUserId(User);


            selection = _db.MovieSelections.Find(selectionID);

            //Set SelectionModel & ViewData
            var selectionModel = new SelectionChoicesEnum(page, "Selection.UserSelection", true, selection, _db, userID);

            if(selectionModel.Status == SelectionChoicesEnum.CompletionStatus.IsIncomplete)
                ViewData["SelectionStatus"] = "*Incomplete Selection, not be added to overall selection";
            else if (selectionModel.Status == SelectionChoicesEnum.CompletionStatus.IsComplete)
                ViewData["SelectionStatus"] = "*Complete Selection";
            else
                ViewData["SelectionStatus"]= "*The list has been deleted";

            return View(selectionModel);
        }

        ///<summary>Sets View data to display completion status of the current selection</summary>
        ///<param name="selectionModel">SelectionModel of the Selection</param>
        ///<returns>A string that contains the completion status of the selection</returns>
        public static string SetViewData(SelectionChoicesEnum selectionModel)
        {
            if (selectionModel.Status == SelectionChoicesEnum.CompletionStatus.IsIncomplete)
                return "*Your Selection is Incomplete and would not be added to overall selection";
            else if(selectionModel.Status == SelectionChoicesEnum.CompletionStatus.IsComplete)
                return "Your Selection for this List is Complete";
            else return "This list has been deleted";
        }

        ///<summary>Creates new selection for the current user for the specified list</summary>
        ///<param name="listID">MovieList's ID to create selection for</param>
        ///<returns>Redirects to the Selection Index View and passes a List ID</returns>
        public IActionResult New(string listID)
        {
            bool isLoggedIn = (User?.Identity != null) && User?.Identity.IsAuthenticated == true;
            if (!isLoggedIn)
            {
                TempData["error"] = "You are not logged in";
                return RedirectToAction("Index", "Lists");
            }

            var list = _db.TopMovieLists.Find(listID);      
            var userID = _userManager.GetUserId(User);

            //Create Selection
            MovieSelection _selection = new MovieSelection();
            _selection.UserID = userID;
            _selection.ListID = listID;
            _selection.ListName = list.ListName;
            _selection.Count= list.Count;
            _db.MovieSelections.Add(_selection);
            _db.SaveChanges();
            TempData["success"] = "Your selection for this list has been created";

            return RedirectToAction("Index", new {listID= listID});
            
        }

        ///<summary>Removes Movie Selection from database</summary>
        ///<param name="selectionID">Movie Selection's ID to be removed</param>
        ///<param name="returnPoint">string value representing Controller and action to return to after processing</param>
        ///<param name="page">Page number of return page</param>
        ///<returns>An IActionResult that redirects to the appropraite returnPoint action</returns>
        public IActionResult Delete(string selectionID, string returnPoint, int page=1)
        {
            var sel= _db.MovieSelections.Find(selectionID);
            var profileUserID = sel.UserID;
            ClearAllChoices(selectionID);
            _db.MovieSelections.Remove(sel);
            _db.SaveChanges();
            TempData["success"] = "Your selection for this list has been deleted";

            return ReturnTo(returnPoint, profileUserID, page);
        }

        ///<summary>uses a Switch to to match return point strings to appropiate actions</summary>
        ///<param name="returnPoint">string value representing Controller and action to return to</param>
        ///<param name="userID">param needed by redirecting action</param>
        ///<param name="page">param needed by redirecting action</param>
        ///<returns>An IActionResult to the appropriate action</returns>
        IActionResult ReturnTo(string returnPoint, string? userID = null, int page = 1)
        {
            switch (returnPoint)
            {
                case "Lists.Index": return RedirectToAction("Index", new { page = page });
                case "UserProfile.Index": return RedirectToAction("Index", "UserProfile", new { profileUserID = userID, page = page });
                case "UserProfile.UserSelections": return RedirectToAction("UserLists", "UserProfile", new { profileUserID = userID, page = page });
                default: return RedirectToAction("Index", new { page = page });
            }
        }

        ///<summary>Clears all choices for Movie Selection from database</summary>
        ///<param name="selectionID">Movie Selection's ID to clear all choices</param>
        ///<returns>Redirects to the Selection Index View and passes a List ID</returns>
        public IActionResult Clear(string selectionID)
        {
            var sel = _db.MovieSelections.Find(selectionID);
            var listID = sel.ListID;
            ClearAllChoices(selectionID);

            _db.SaveChanges();
            TempData["success"] = "Your choices for this selection has been cleared";
            return RedirectToAction("Index", new { listID = listID });
        }

        ///<summary>Removes all choices for Movie Selection from database</summary>
        ///<param name="selectionID">Movie Selection's ID to remove all choices</param>
        void ClearAllChoices(string selectionID)
        {
            var choiceEnum = from c in _db.MovieChoices where c.SelectionID == selectionID select c;

            foreach (var c in choiceEnum)
            {
                _db.MovieChoices.Remove(c);
            }
        }

        ///<summary>Moves choices to rank above or below in a Movie Selection</summary>
        ///<param name="selectionID">Movie Selection's ID</param>
        ///<param name="listID">MovieList's ID</param>
        ///<param name="choiceID">Movie Choice's ID to change Rank</param>
        ///<param name="direction">integer value i.e +1 or -1, that represents direction to change Rank</param>
        ///<returns>Redirects to the Selection Index View and passes a List ID</returns>
        public IActionResult EditRank(string choiceID, string listID, string selectionID, int direction)
        {
            var choice = _db.MovieChoices.Find(choiceID);
            var list = _db.TopMovieLists.Find(listID);

            int rank = choice.Rank;
            int nextRank = rank + direction;

            if (nextRank > list.Count)
                nextRank = 1;
            else if (nextRank <= 0)
                nextRank = list.Count;

            //exchange rank of choice in intended rank if it exists
            var nextEnum = from c in _db.MovieChoices where c.Rank == nextRank && c.SelectionID==selectionID select c;

            if (nextEnum.Any())
            {
                string id = nextEnum.First().ChoiceID;
                MovieChoice nextChoice = _db.MovieChoices.Find(id);
                nextChoice.Rank = rank;
                _db.MovieChoices.Update(nextChoice);
            }

            choice.Rank = nextRank;

            _db.MovieChoices.Update(choice);
            _db.SaveChanges();
            return RedirectToAction("Index", new { listID = listID });
        }

        ///<summary>Stacks all choices to the top of a Movie Selection</summary>
        ///<param name="selectionID">Movie Selection's ID</param>
        ///<param name="listID">MovieList's ID</param>
        ///<returns>Redirects to the Selection Index View and passes a List ID</returns>
        public IActionResult StackToTop(string selectionID, string listID)
        {
            var choiceEnum = from c in _db.MovieChoices where c.SelectionID == selectionID orderby c.Rank ascending select c;
            int i = 1;
            foreach (var c in choiceEnum)
            {
                if (c.Rank > i)
                {
                    var choice = _db.MovieChoices.Find(c.ChoiceID);
                    choice.Rank = i;
                    _db.MovieChoices.Update(choice);
                }

                i++;
            }

            _db.SaveChanges();
            return RedirectToAction("Index", new { listID = listID });
        }

        ///<summary>Stacks all choices to the bottom of a Movie Selection</summary>
        ///<param name="selectionID">Movie Selection's ID</param>
        ///<param name="listID">MovieList's ID</param>
        ///<returns>Redirects to the Selection Index View and passes a List ID</returns>
        public IActionResult StackToBottom(string selectionID, string listID)
        {
            var choiceEnum = from c in _db.MovieChoices where c.SelectionID == selectionID orderby c.Rank descending select c;
            var list = _db.TopMovieLists.Find(listID);
            int i = list.Count;
            foreach (var c in choiceEnum)
            {
                if (c.Rank < i)
                {
                    var choice = _db.MovieChoices.Find(c.ChoiceID);
                    choice.Rank = i;
                    _db.MovieChoices.Update(choice);
                }

                i--;

            }

            _db.SaveChanges();
            return RedirectToAction("Index", new { listID = listID });
        }

        ///<summary>Gets all choices in a selection</summary>
        ///<param name="selectionID">Movie Selection's ID</param>
        ///<param name="db">database context</param>
        ///<returns>A list of Movie Choices</returns>
        public static List<MovieChoice> GetChoices(string selectionID, ApplicationContext db)
        {
            var choiceEnum= from c in db.MovieChoices where c.SelectionID==selectionID orderby c.Rank ascending select c ;
            List<MovieChoice> choices= new List<MovieChoice>();

            foreach(MovieChoice c in choiceEnum) choices.Add(c);
            return choices;
        }

        ///<summary>Create choiceModels from a list of choices</summary>
        ///<param name="userID">current user's ID</param>
        ///<param name="db">database context</param>
        ///<returns>A list of ChoiceModels</returns>
        public static List<ChoiceModel> GetChoiceModels(List<MovieChoice> choices, ApplicationContext db, string userID)
        {
            List<ChoiceModel>  Models = new List<ChoiceModel>();

            foreach(var a in choices)
            {
                var favEnum = from f in db.FavouriteMovies where f.MovieID == a.MovieID && f.UserID == userID select f;
                Models.Add(new ChoiceModel(a, favEnum.Any()));
            }

            return Models;
        }

        ///<summary>Class that stores data for adding choices to selection</summary>
        public class AddToModel
        {
            public string? SelectionID { get; set; }
            public int? Rank { get; set; }
            public string? SearchString { get; set; }
            public bool IsNull { get { return SelectionID == null && Rank == null && SearchString == null; } set { } }
            public AddToModel(string? selectionID, int? rank, string? searchString = null)
            {
                SelectionID = selectionID;
                Rank = rank;
                SearchString = searchString;
            }

            public AddToModel() 
            {
                SelectionID = null;
                Rank = null;
                SearchString = null;
            }

        }

        ///<summary>Class for displaying an enumeration of Selection Choices</summary>
        public class SelectionChoicesEnum:DisplayEnum
        {
            public MovieSelection Selection{ get; set; }
            public enum CompletionStatus {IsIncomplete, IsComplete, IsDeleted}
            public CompletionStatus Status { get; set; }
            public List<ChoiceModel> Models { get; set; }=new List<ChoiceModel>();
            public bool DisplayComplete { get; set; } = true;
            public int Page { get; set; } = 1;
            public string ReturnPoint { get; set; }
            public bool IsUser { get; set; }
            public SelectionChoicesEnum(List<MovieChoice> models, int page, string returnPoint, bool displayComplete, MovieSelection selection, ApplicationContext db, string userID) : base(models, page, returnPoint, displayComplete)
            {
                
                Selection = selection;
                Models = GetChoiceModels(models, db, userID);
                Page = page;
                DisplayComplete = displayComplete;
                ReturnPoint = returnPoint;
                IsUser= (selection.UserID== userID);
                if(Selection.ListID==null) Status = CompletionStatus.IsDeleted;
                else if (Models == null || Models.Count() < selection.Count) Status = CompletionStatus.IsIncomplete;
                else if(Models.Count() == selection.Count) Status = CompletionStatus.IsComplete;
            }

            public SelectionChoicesEnum(int page, string returnPoint, bool displayComplete, MovieSelection selection, ApplicationContext db, string userID)
            {
                List<MovieChoice> models = GetChoices(selection.SelectionID, db);
                Selection = selection;
                Models = GetChoiceModels(models, db, userID);
                Page = page;
                DisplayComplete = displayComplete;
                ReturnPoint = returnPoint;
                IsUser = (selection.UserID == userID);
                if (Selection.ListID == null) Status = CompletionStatus.IsDeleted;
                else if (Models == null || Models.Count() < selection.Count) Status = CompletionStatus.IsIncomplete;
                else if (Models.Count() == selection.Count) Status = CompletionStatus.IsComplete;
            }
        }

        ///<summary>Class for storing & displaying Choices</summary>
        public class ChoiceModel
        {
            public MovieChoice Choice { get; set; }
            public bool IsFavourite { get; set; }

            public ChoiceModel(MovieChoice choice, bool isFavourite)
            {
                Choice = choice;
                IsFavourite= isFavourite;
            }
        }
    }
}
