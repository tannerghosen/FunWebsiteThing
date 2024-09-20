using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Principal;

namespace FunWebsiteThing.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string Result { get; set; }

        private SQLStuff _s = new SQLStuff();

        private readonly ILogger<IndexModel> _logger;

        public LoginModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public void OnPost()
        {
            Random r = new Random();
            int sid = r.Next(999999999);
            Console.WriteLine(sid);
            bool result = _s.Login(Username, Password, sid);
            if ((!string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password)) && HttpContext.Session.GetInt32("IsLoggedIn") != 1)
            {
                if (result == true)
                {
                    HttpContext.Session.SetString("Username", Username);
                    HttpContext.Session.SetInt32("UserId", _s.GetUserID(Username));
                    HttpContext.Session.SetInt32("SessionId", sid);
                    HttpContext.Session.SetInt32("IsLoggedIn", 1);
                    Result = "Login successful. Logged in as: " + Username + ".";
                    Console.WriteLine(HttpContext.Session.GetString("Username") + " " + HttpContext.Session.GetInt32("UserId") + " " + HttpContext.Session.GetInt32("SessionId") + " " + HttpContext.Session.GetInt32("IsLoggedIn"));
                }
                else
                {
                    Result = "Invalid login";
                }
            }
            TempData["Result"] = Result;
            Console.WriteLine(TempData["Result"]);
        }
    }
}