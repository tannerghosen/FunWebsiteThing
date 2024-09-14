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

        public void OnGet()
        {
        }

        public void OnPost()
        {
            string username = HttpContext.Session.GetString("Username") ?? "Anonymous";
            _s.AddComment(Comment, username);
        }
    }
}
