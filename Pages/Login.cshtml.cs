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

        private SessionManager _s;

        private readonly ILogger<IndexModel> _logger;

        public LoginModel(ILogger<IndexModel> logger, SessionManager s)
        {
            _logger = logger;
            _s = s;
        }

        public void OnGet()
        {

        }

        public async void OnPost()
        {
            int sid = _s.SID();
            (bool result, bool error) = await SQL.Accounts.Login(Username, Password, sid);
            if ((!string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password)) && HttpContext.Session.GetInt32("IsLoggedIn") != 1)
            {
                if (result == true)
                {
                    _s.Login(Username, SQL.Accounts.GetUserID(Username), sid);
                    Result = "Login successful. Logged in as: " + Username + ".";
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
        }
    }
}