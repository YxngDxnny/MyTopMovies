using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyTopMovies.Areas.Identity.Data;
using MyTopMovies.Data;
using MyTopMovies.Models;
using System.Collections.Generic;
using System.Xml.Linq;
using static MyTopMovies.Controllers.ListsController;

namespace MyTopMovies.Controllers
{
    ///<summary>Controller Class that manages and handle user profiles</summary>
    public class UserProfileController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserProfileController(ApplicationContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        ///<summary>Displays search for users profile</summary>
        ///<returns>The UserProfile Index View and passes an empty UserProfile.UserEnum type</returns>
        public IActionResult Index()
        {
            return View(new UserEnum());
        }

        ///<summary>Searches users for those whose name matches the search string queried</summary>
        ///<param name="page">page number of current search</param>
        ///<param name="searchString">Search string to query against Users table</param>
        ///<returns>The UserProfile Index View and passes a UserProfile.UserEnum type</returns>
        public IActionResult Search(string searchString, int page=1)
        {
            var users = _db.Users.FromSqlRaw($"SELECT * FROM UserMngt.Users where FullName Like '%" + searchString + "%'")
            .ToList();

            List<UserModel> models = new List<UserModel>();

            foreach (var u in users)
            {
                UserModel model = new UserModel(new UserProfileModel(u));
                models.Add(model);
            }
            UserEnum indexModel = new UserEnum(models, page, "UserProfile.ListSelections", true);

            TempData["searchString"] = searchString;
            return View("~/Views/UserProfile/Index.cshtml", indexModel);
        }

        ///<summary>creates model to display user selections and created lists preview</summary>
        ///<param name="profileUserID">user's ID to display profile page for</param>
        ///<returns>The UserProfile Index View and passes a UserProfile.UserIndexModel type</returns>
        public IActionResult UserIndex(string profileUserID)
        {
            List<SelectionModel> selections = new List<SelectionModel>();
            List<ListDisplayModel> lists = new List<ListDisplayModel>();
            string userID = _userManager.GetUserId(User);

            var selQ = from s in _db.MovieSelections where s.UserID == profileUserID select s;
            var listQ = from l in _db.TopMovieLists where l.UserID == profileUserID select l;

            int i = 0;
            foreach(var s in selQ)
            {
                if (i == 6) break;
                selections.Add(new SelectionModel( s, _db));
                i++;
            }

            i = 0;
            foreach (var l in listQ)
            {
                if (i == 6) break;
                lists.Add(new ListDisplayModel(l, _db, userID));
                i++;
            }

            ApplicationUser _profileUser = _db.Users.Find(profileUserID);
            UserProfileModel _profile = new UserProfileModel(_profileUser);
            bool isUser = profileUserID == userID;

            UserSelectionEnum selEnum = new UserSelectionEnum(selections, 1, "UserProfile.UserIndex", false, _profile, isUser);
            UserListEnum listEnum = new UserListEnum(lists, 1, "UserProfile.UserIndex", false, _profile);

            UserIndexModel model = new UserIndexModel(selEnum, listEnum, _profile, isUser);
            return View(model);
        }

        ///<summary>display all user profiles that made selections for a specified lists</summary>
        ///<param name="listID">list's ID to display all user profiles</param>
        ///<returns>The UserProfile Index View and passes a UserProfile.UserEnum type</returns>
        public IActionResult ListSelections(string listID, int page=1)
        {
            TopMovieList list = _db.TopMovieLists.Find(listID);

            var selEnum = from s in _db.MovieSelections where s.ListID == listID select s;

            List<UserModel> models = new List<UserModel>();

            foreach(var s in selEnum)
            {
                int choiceCount = SelectionController.GetChoices(s.SelectionID, _db).Count();
                int listCount = _db.TopMovieLists.Find(s.ListID).Count;
                if (choiceCount==listCount)
                {
                    ApplicationUser _user = _db.Users.Find(s.UserID);
                    UserProfileModel profile = new UserProfileModel(_user);
                    models.Add(new UserModel(profile, s.SelectionID));
                }
                
            }

            UserEnum indexModel = new UserEnum(models, page, "UserProfile.ListSelections", true, listID);
            return View("~/Views/UserProfile/Index.cshtml", indexModel);
            
        }

        ///<summary>creates model to display all created lists by users</summary>
        ///<param name="profileUserID">user's ID to display all created lists for</param>
        ///<param name="page">page number</param>
        ///<returns>View and passes a UserProfile.UserListEnum type</returns>
        public IActionResult UserLists(string profileUserID, int page = 1)
        {
            var listQ = from l in _db.TopMovieLists where l.UserID == profileUserID select l;
            List<ListDisplayModel> models = new List<ListDisplayModel>();
            string userID = _userManager.GetUserId(User);
            foreach (var l in listQ)
            {
                models.Add(new ListDisplayModel(l, _db, userID));
            }

            UserListEnum userListEnum = new UserListEnum(models, page, "UserProfile.UserLists", true, new UserProfileModel(_db.Users.Find(profileUserID)));
            return View(userListEnum);
        }

        ///<summary>creates model to display all selections made by user</summary>
        ///<param name="profileUserID">user's ID to display all created lists for</param>
        ///<param name="page">page number</param>
        ///<returns>View and passes a UserProfile.UserSelectionEnum type</returns>
        public IActionResult UserSelections(string profileUserID, int page=1)
        {
            var selEnum = from s in _db.MovieSelections where s.UserID == profileUserID select s;
            List<SelectionModel> models = new List<SelectionModel>();
            foreach(var s in selEnum)
            {
                models.Add(new SelectionModel(s, _db));
            }

            UserSelectionEnum userSelectionEnum = new UserSelectionEnum(models, page, "UserProfile.UserSelections", true, new UserProfileModel( _db.Users.Find(profileUserID)), profileUserID==_userManager.GetUserId(User));
            return View(userSelectionEnum);
        }

        ///<summary>class user to display a users selection</summary>
        public class SelectionModel
        {
            public MovieSelection Selection { get; set; }
            public string? CreatorName { get; set; }
            public string? CreatorID{ get; set; }
            public SelectionModel(MovieSelection selection, ApplicationContext db)
            {
                Selection = selection;
                var list = db.TopMovieLists.Find(selection.ListID);

                if (list == null)
                {
                    CreatorName = "(Deleted List)";
                    CreatorID = null;
                }
                    
                else
                {
                    CreatorName = db.Users.Find(list.UserID).FullName; 
                    CreatorID = list.UserID;
                }
            }
        }

        ///<summary>Class for displaying an enumeration of Selections made by a user</summary>
        public class UserSelectionEnum:DisplayEnum
        {
            public List<SelectionModel> Models { get; set; }= new List<SelectionModel>();
            public UserProfileModel Profile { get; set; }
            public bool IsUser { get; set; }
            public bool DisplayComplete { get; set; } = true;
            public int Page { get; set; } = 1;
            public string ReturnPoint;
            public UserSelectionEnum(List<SelectionModel> models, int page, string returnPoint, bool displayComplete, UserProfileModel profile, bool isUser) : base(models, page, returnPoint, displayComplete)
            {
                Models = models;
                Profile = profile;
                IsUser = isUser;
                DisplayComplete = displayComplete;
                Page = page;
                ReturnPoint = returnPoint;
            }
        }

        ///<summary>Class for displaying an enumeration of Lists made by a user</summary>
        public class UserListEnum: ListsController.ListsEnum
        {
            public UserProfileModel Profile { get; set; }
            public UserListEnum( List<ListDisplayModel> models, int page, string returnPoint, bool displayComplete, UserProfileModel profile): base(models, page, returnPoint, displayComplete)
            {
                Models = models;
                DisplayComplete = displayComplete;
                Page = page;
                Profile = profile;
                ReturnPoint = returnPoint;
            }
        }

        ///<summary>Class for displaying a user profile</summary>
        public class UserIndexModel
        { 
            public UserProfileModel Profile { get; set; }
            public UserSelectionEnum SelectionEnum { get; set; }
            public UserListEnum ListEnum { get; set; }
            public bool IsUser { get; set; }
            public UserIndexModel(UserSelectionEnum selections, UserListEnum listEnum, UserProfileModel profile, bool isUser)
            {
                SelectionEnum = selections;
                ListEnum = listEnum;
                Profile = profile;
                IsUser = isUser;
            }   
        }

        //<summary>Class for Storing a user profile</summary>
        public class UserProfileModel
        {
            public string UserFullName { get; set; }
            public string UserID { get; set; }
            public byte[]? Picture { get; set; }

            public UserProfileModel(ApplicationUser user)
            {
                UserFullName = user.FullName;
                UserID = user.Id;
                Picture = user.Picture;
            }
        }

        //<summary>Class for displaying an individual users</summary>
        public class UserModel
        {
            public UserProfileModel Profile { get; set; }
            public string? SelectionID { get; set; }

            public UserModel(UserProfileModel profile, string? selectionID=null )
            {
                Profile = profile;
                SelectionID = selectionID;
            }
        }

        ///<summary>Class for displaying an enumeration of Users</summary>
        public class UserEnum : DisplayEnum
        {
            public List<UserModel> Models { get; set; }= new List<UserModel>();
            public bool DisplayComplete { get; set; } = true;
            public int Page { get; set; } = 1;
            public string ReturnPoint { get; set; }
            public string? ListID { get; set; }
            public UserEnum(List<UserModel> models, int page, string returnPoint, bool displayComplete, string listID=null) : base(models, page, returnPoint, displayComplete)
            {
                Models = models;
                DisplayComplete = displayComplete;
                Page = page;
                ReturnPoint = returnPoint;
                ListID = listID;
            }
            public UserEnum()
            {
                Models = new List<UserModel>();
            }

        }


    }
}
