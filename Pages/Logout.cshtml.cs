using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class LogoutModel : PageModel
    {
        private SessionController _ac;

        public LogoutModel(SessionController s)
        {
            _ac = s;
        }
        public void OnGet()
        {
        }
        public void OnPost()
        {
            Console.WriteLine("Logging out this user: " + HttpContext.Session.GetString("Username") + " " + HttpContext.Session.GetInt32("UserId") + " " + HttpContext.Session.GetInt32("SessionId") + " " + HttpContext.Session.GetInt32("IsLoggedIn"));
            _ac.Logout();

            Response.Redirect("/Logout");
        }
    }
}
