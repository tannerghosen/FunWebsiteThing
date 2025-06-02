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
            if (Post < 1) // if post is less than 1 we went too far back
            {
                Post = 1;
            }
            else if (Post > SQL.Blog.GetBlogPostCount()) // if post is greater than the total amount of posts, we went too far forward
            {
                Post = SQL.Blog.GetBlogPostCount();
            }
            (Title, Message) = SQL.Blog.GetBlogPost(Post); //  Get the post to be displayed in the page
        }

        public async Task<IActionResult> OnPost()
        {
            Post = int.TryParse(Request.Form["CS"], out int cs) ? cs : 0; // What post this comment belongs to (CS input in form on page)
            string username = HttpContext.Session.GetString("Username") ?? "Anonymous"; // if the user is not logged in, use anonymous
            await SQL.Comments.AddComment(Comment, username, Post);

            return RedirectToPage("/Blog", new { post = Post});
        }

        public async Task<IActionResult> OnPostDelete(int? commentid)
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                await SQL.Comments.DeleteComment(commentid);
            }

            return RedirectToPage("/Blog", new { post = Post });
        }
    }
}
