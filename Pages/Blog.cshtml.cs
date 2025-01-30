using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;
#pragma warning disable CS8618

namespace FunWebsiteThing.Pages
{
    public class BlogModel : PageModel
    {
        [BindProperty]
        public string Comment { get; set; }

        [BindProperty]
        public int Post { get; set; }

        [BindProperty]
        public string Title { get; set; }

        [BindProperty]
        public string Message { get; set; }

        public void OnGet()
        {
            Post = Convert.ToInt32(Request.Query["post"]);
            if (Post < 1)
            {
                Post = 1;
            }
            else if (Post > SQL.Blog.GetBlogPostCount())
            {
                Post = SQL.Blog.GetBlogPostCount();
            }
            (Title, Message) = SQL.Blog.GetBlogPost(Post);
        }

        public async Task<IActionResult> OnPost()
        {
            Post = int.TryParse(Request.Form["CS"], out int cs) ? cs : 0;
            Title = Request.Form["Title"];
            Message = Request.Form["Message"];
            string username = HttpContext.Session.GetString("Username") ?? "Anonymous";
            await SQL.Comments.AddComment(Comment, username, Post);

            return RedirectToPage("/Blog", new { post = Post});
        }

        public async Task<IActionResult> OnPostDelete(int? commentid)
        {
            Post = int.TryParse(Request.Form["CS"], out int post) ? post : 0;
            Title = Request.Form["Title"];
            Message = Request.Form["Message"];
            Logger.Write("CommentSection is " + Post + ", TryParse is " + post);
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                await SQL.Comments.DeleteComment(commentid);
            }

            return RedirectToPage("/Blog", new { post = Post });
        }
    }
}
