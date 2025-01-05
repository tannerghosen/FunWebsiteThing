using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class LogoutModel : PageModel
    {
        private SessionManager _s;

        public LogoutModel(SessionManager s)
        {
            _s = s;
        }
        public void OnGet()
        {
        }
        public void OnPost()
        {
            Logger.Write("Logging out this user: " + HttpContext.Session.GetString("Username") + " " + HttpContext.Session.GetInt32("UserId") + " " + HttpContext.Session.GetInt32("SessionId") + " " + HttpContext.Session.GetInt32("IsLoggedIn"), "LOGOUT");
            _s.Logout();

            Response.Redirect("/Index");
        }
    }
}
