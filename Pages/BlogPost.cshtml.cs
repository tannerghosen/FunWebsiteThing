using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
#pragma warning disable CS8618

namespace FunWebsiteThing.Pages
{
    public class BlogPostModel : PageModel
    {
        [BindProperty]
        public string Title { get; set; }

        [BindProperty]
        public string Message { get; set; }

        [BindProperty]
        public string BlogPostId { get; set; }

        public void OnGet()
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                Response.Redirect("/Index");
            }
        }
        public async Task<IActionResult> OnPost()
        {
            int blogid = SQL.Blog.GetBlogPostCount() + 1;
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                await SQL.Blog.AddBlogPost(Title, Message);
            }
            return RedirectToPage("/Blog", new { post = blogid });
        }

        public async Task<IActionResult> OnPostDelete()
        {
            int id = int.TryParse(Request.Form["BlogPostId"], out int blogpostid) ? blogpostid : 0;
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                await SQL.Blog.DeleteBlogPost(id);
            }

            return RedirectToPage("/BlogPost");
        }
    }
}
