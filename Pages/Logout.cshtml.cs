using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class LogoutModel : PageModel
    {
        private SessionController _s;

        public LogoutModel(SessionController s)
        {
            _s = s;
        }
        public void OnGet()
        {
        }
        public void OnPost()
        {
            Console.WriteLine("Logging out this user: " + HttpContext.Session.GetString("Username") + " " + HttpContext.Session.GetInt32("UserId") + " " + HttpContext.Session.GetInt32("SessionId") + " " + HttpContext.Session.GetInt32("IsLoggedIn"));
            _s.Logout();

            Response.Redirect("/Logout");
        }
    }
}
