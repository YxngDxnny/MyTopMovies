using Microsoft.AspNetCore.Mvc;
using MyTopMovies.Models;
using System.Diagnostics;

namespace MyTopMovies.Controllers
{
    /// <summary>
    /// Controller class that handles the home page
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        ///<summary>Displays the home page</summary>
        ///<returns>View for the home page</returns>
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}