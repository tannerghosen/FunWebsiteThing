using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;
using System.Xml.Linq;

namespace FunWebsiteThing.Pages
{
    public class CommentsModel : PageModel
    {
        [BindProperty]
        public string Comment { get; set; }

        private SQLStuff _s = new SQLStuff();

        [BindProperty]
        public int CommentSection { get; set; }

        public void OnGet()
        {
            CommentSection = Convert.ToInt32(Request.Query["cs"]);
        }

        public void OnPost()
        {
            CommentSection = int.TryParse(Request.Form["CS"], out int cs) ? cs : 0;
            string username = HttpContext.Session.GetString("Username") ?? "Anonymous";
            _s.AddComment(Comment, username, CommentSection);
        }

        public IActionResult OnPostDelete(int? commentid)
        {
            CommentSection = int.TryParse(Request.Form["CS"], out int cs) ? cs : 0;
            Logger.Write("CommentSection is " + CommentSection + ", TryParse is " + cs);
            if (HttpContext.Session.GetInt32("IsAdmin") == 1)
            {
                _s.DeleteComment(commentid);
            }

            return RedirectToPage("/Comments", new { cs = CommentSection });
        }
    }
}
