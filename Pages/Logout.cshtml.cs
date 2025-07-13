using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class LogoutModel : PageModel
    {
        private SessionManager _s;
        private AccountController _a;
        public LogoutModel(SessionManager s, AccountController a)
        {
            _s = s;
            _a = a;
        }
        public void OnGet()
        {
            if (HttpContext.Session.GetInt32("IsLoggedIn") != 1)
            {
                Response.Redirect("/Index");
            }
        }
        public async Task<IActionResult> OnPost()
        {
            _a.Logout();
            Logger.Write("Logging out this user: " + HttpContext.Session.GetString("Username") + " " + HttpContext.Session.GetInt32("UserId") + " " + HttpContext.Session.GetInt32("SessionId") + " " + HttpContext.Session.GetInt32("IsLoggedIn"), "LOGOUT");

            return Redirect("/Index");
        }
    }
}
