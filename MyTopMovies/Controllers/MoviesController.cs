using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyTopMovies.Areas.Identity.Data;
using MyTopMovies.Data;
using MyTopMovies.Models;
using System.Net.Http.Headers;
using System.Text;

namespace MyTopMovies.Controllers
{
    ///<summary>Class for managing and displaying movies</summary>
    public class MoviesController : Controller
    {
        /// <summary> URL for quering movies against a search string</summary>
        private const string queryURL = "https://api.themoviedb.org/3/search/movie";

        /// <summary> URL parameters for HTTP requests </summary>
        private static string urlParameters="";

        /// <summary> URL for finding movies with their TMBD ID </summary>
        private const string findURL = "https://api.themoviedb.org/3/movie/s";

        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public MoviesController(ApplicationContext db, UserManager<ApplicationUser> userManager)
        {
            urlParameters = "?api_key=" + ApiKeys.TMDBApiKey + "&language=en-US";
            _db = db;
            _userManager = userManager;
        }

        ///<summary>Display specified movie's details</summary>
        ///<param name="movieID">ID of movie to display details</param>
        ///<returns>The View and passes a Movie type or Not found if movie not found</returns>
        public IActionResult Index(int movieID)
        {
            Movie movie = GetMovie(movieID);
            
            if (movie==null)
            {
                return NotFound();
            }
            
            movie.poster_path = "https://image.tmdb.org/t/p/w500" + movie.poster_path;
            return View(movie);
           
        }

        ///<summary>Display search and search results for movie queries</summary>
        ///<param name="returnPoint">string value representing Controller and action to return to after processing</param>
        ///<param name="page">Page number of return page</param>
        ///<param name="selectionID">ID of the Selection to AddMovie as a choice to</param>
        ///<param name="rank">intended rank for the selection to Add Movie as a choice to</param>
        ///<returns>The View and passes an empty searchMovieModel type</returns>
        public IActionResult Search(string? selectionID= null, int? rank=null, string? returnPoint= null, int page = 1)
        {
            SelectionController.AddToModel addTo = new SelectionController.AddToModel(selectionID, rank, null);
            MovieSearchModel movieSearchModel = new MovieSearchModel(addTo, returnPoint, page);

            return View(movieSearchModel);
        }

        ///<summary>post's a search query and creates a searchMovieModel type</summary>
        ///<param name="searchString">Search string to query against Movie Lists table</param>
        ///<param name="returnPoint">string value representing Controller and action to return to after processing</param>
        ///<param name="page">Page number of return page</param>
        ///<param name="selectionID">ID of the Selection to AddMovie as a choice to</param>
        ///<param name="rank">intended rank for the selection to Add Movie as a choice to</param>
        ///<returns>The Search View and passes a searchMovieModel type</returns>
        public IActionResult SearchPost(String searchString, string? selectionID= null, int? rank= null, string? returnPoint = null, int page=1)
        {
            TempData["searchString"] = searchString;

            //Empty search box
            if (searchString == null || searchString.Length == 0)
            {
                TempData["error"] = "The Searchbox is empty";
                return RedirectToAction("Search", new { selectionID = selectionID, rank = rank });
            }

            //Edit SearchString for url
            var builder = new StringBuilder();

            foreach (char c in searchString)
            {
                if (c == ' ') builder.Append("%20");
                else builder.Append(c);
            }
            string query = "&query=" + builder.ToString() + "&page=1&include_adult=false";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(queryURL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            List<Movie> movies = new List<Movie>();
            HttpResponseMessage response = client.GetAsync(urlParameters + query).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                var movieSearchResult = response.Content.ReadAsAsync<MovieSearchResult>().Result;
                movies = movieSearchResult.results;

                if (movies.Count() == 0)
                    TempData["error"] = "No movie matches your search";
            }
            else
            {
                client.Dispose();
                return Problem(response.ReasonPhrase, null, (int)response.StatusCode);
            }

            // Dispose once all HttpClient calls are complete.
            client.Dispose();

            if (returnPoint == null) returnPoint = "Movies.Search";

            SelectionController.AddToModel addTo= new SelectionController.AddToModel(selectionID, rank, searchString);
            return View("~/Views/Movies/Search.cshtml", new MovieSearchModel(movies, addTo, _db, _userManager.GetUserId(User), returnPoint, page));
        }

        public static Movie GetMovie(int movieID)
        {
            string query = movieID.ToString() + urlParameters;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(findURL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            Movie movie = new Movie();
            HttpResponseMessage response = client.GetAsync(query).Result;

            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                var movieSearchResult = response.Content.ReadAsAsync<Movie>().Result;
                movie = movieSearchResult;
            }
            else
            {
                client.Dispose();
                return null;
            }

            movie.poster_path = "https://image.tmdb.org/t/p/w500" + movie.poster_path;

            // Dispose once all HttpClient calls are complete.
            client.Dispose();

            return movie;
        }

        ///<summary>Class for displaying and storing movie searches</summary>
        public class MovieSearchModel
        {
            public MovieSearchEnum MovieEnum { get; set; } = new MovieSearchEnum();
            public SelectionController.AddToModel AddTo { get; set; }
            public MovieSearchModel(SelectionController.AddToModel addTo, string? returnPoint= null, int page= 1)
            {
                AddTo= addTo;
                MovieEnum.ReturnPoint = returnPoint;
                MovieEnum.Page = page;
            }
            public MovieSearchModel(List<Movie> movies, SelectionController.AddToModel addTo, ApplicationContext db, string userID, string? returnPoint = null, int page = 1)
            {
                List<MovieModel>  models = new List<MovieModel>();


                foreach (var m in movies)
                {
                    var favEnum = from f in db.FavouriteMovies where f.MovieID == m.id.ToString() && f.UserID == userID select f;
                    models.Add(new MovieModel(m, favEnum.Any()));
                }
                AddTo = addTo;
                MovieEnum = new MovieSearchEnum(models, addTo.SearchString, true, page, returnPoint, AddTo);
            }
        }

        ///<summary>Class for displaying individual movies</summary>
        public class MovieModel
        {
            public Movie Movie { get; set; }

            public bool IsFavourite { get; set; }
            public MovieModel(Movie movie, bool isFavourite)
            {
                Movie = movie;
                IsFavourite = isFavourite;
            }
        }

        ///<summary>Class for displaying an enumeration Movies</summary>
        public class MovieSearchEnum: DisplayEnum
        {
            public List<MovieModel> Models { get; set; }
            public string SearchString { get; set; }
            public bool DisplayComplete { get; set; } = true;
            public int Page { get; set; } = 1;
            public string ReturnPoint { get; set; }

            public SelectionController.AddToModel AddTo { get; set; }
            public MovieSearchEnum(List<MovieModel> models, string searchString, bool displayComplete, int page, string returnPoint, SelectionController.AddToModel addTo) :base(models, page, returnPoint, displayComplete)
            {
                Models = models;
                SearchString = searchString;
                DisplayComplete = displayComplete;
                Page = page;
                ReturnPoint = returnPoint;
                AddTo= addTo;
            }

            public MovieSearchEnum()
            {
                Models = new List<MovieModel>();
                AddTo= new SelectionController.AddToModel();
            }
        }

    }
}
