using System.Collections;
using static MyTopMovies.Controllers.ListsController;

namespace MyTopMovies
{
    ///<summary>Class for Display settings</summary>

    public static class Display
    {
        public const int FullItemCount = 20, PreviewItemCount = 5;
    }

    ///<summary>Base Class for View Enumerations</summary>
    public class DisplayEnum
    {
        public IEnumerable Models { get; set; }
        public int Page { get; set; } = 1;
        public string ReturnPoint { get; set; }
        public bool DisplayComplete { get; set; } = true;

        public DisplayEnum(IEnumerable models, int page, string returnPoint, bool displayComplete) 
        {
            Models = models; 
            Page = page;
            ReturnPoint = returnPoint;
            DisplayComplete = displayComplete;
        }

        public DisplayEnum()
        {

        }
    }
}