using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class LogoutModel : PageModel
    {
        public void OnGet()
        {
        }
        public void OnPost()
        {
            Console.WriteLine("Logging out this user: " + HttpContext.Session.GetString("Username") + " " + HttpContext.Session.GetInt32("UserId") + " " + HttpContext.Session.GetInt32("SessionId") + " " + HttpContext.Session.GetInt32("IsLoggedIn"));
            HttpContext.Session.SetString("Username", "");
            HttpContext.Session.SetInt32("IsLoggedIn", 0);
            HttpContext.Session.SetInt32("UserId", -999999999);
            HttpContext.Session.SetInt32("SessionId", -999999999);
            Response.Redirect("/Logout");
        }
    }
}
