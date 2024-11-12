using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        private SQLStuff _s;
        private SessionController _ac;

        private readonly ILogger<IndexModel> _logger;

        public LoginModel(ILogger<IndexModel> logger, SessionController s, SQLStuff sq)
        {
            _logger = logger;
            _ac = s;
            _s = sq;
        }

        public void OnGet()
        {

        }

        public void OnPost()
        {
            Random r = new Random();
            int sid = r.Next(999999999);
            Console.WriteLine(sid);
            (bool result, bool error) = _s.Login(Username, Password, sid);
            if ((!string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password)) && HttpContext.Session.GetInt32("IsLoggedIn") != 1)
            {
                if (result == true)
                {
                    _ac.Login(Username, _s.GetUserID(Username), sid);
                    Result = "Login successful. Logged in as: " + Username + ".";
                    Console.WriteLine(HttpContext.Session.GetString("Username") + " " + HttpContext.Session.GetInt32("UserId") + " " + HttpContext.Session.GetInt32("SessionId") + " " + HttpContext.Session.GetInt32("IsLoggedIn"));
                    RedirectToPage();
                }
                else if (result == false && error != true)
                {
                    Result = "Invalid login";
                }
                else if (error == true)
                {
                    Result = "An error occured while logging in";
                }
            }
            TempData["Result"] = Result;
            Console.WriteLine(TempData["Result"]);
        }
    }
}