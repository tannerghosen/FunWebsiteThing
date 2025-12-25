using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class AdminModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public AdminModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        public void OnGet()
        {
            if (HttpContext.Session.GetInt32("IsAdmin") != 1 && !SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                Response.Redirect("/Index");
            }
        }

        public async Task<IActionResult> OnPostDelete(int? userid)
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId"))) // is the user admin and does the userid in session check out as admin? if so, delete user requested.
            {
                await SQL.Admin.DeleteUser(userid);
            }

            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostAdmin(int? userid)
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && HttpContext.Session.GetInt32("UserId") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId"))) // is the user admin and is it super admin doing this and does the userid in session check out as admin? if so, make the requested userid an admin
            {
                await SQL.Admin.AdminUser(userid);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostResetStats()
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && HttpContext.Session.GetInt32("UserId") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId"))) // is the user admin and is it super admin doing this and does the userid in session check out as admin? if so, make the requested userid an admin
            {
                Statistics.ResetStats();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnbanUser(int? userid)
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && HttpContext.Session.GetInt32("UserId") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId"))) // is the user admin and is it super admin doing this and does the userid in session check out as admin? if so, make the requested userid an admin
            {
                await SQL.Admin.UnbanUser(userid);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBanUser(int? userid)
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && HttpContext.Session.GetInt32("UserId") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId"))) // is the user admin and is it super admin doing this and does the userid in session check out as admin? if so, make the requested userid an admin
            {
                await SQL.Admin.BanUser(userid, "You have been banned.", DateTime.Now.AddYears(999));
            }
            return RedirectToPage();
        }
    }
}
