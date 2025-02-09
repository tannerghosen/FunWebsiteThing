using FunWebsiteThing.SQL;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
#pragma warning disable CS8618

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

        [BindProperty]
        public string Method { get; set; }

        private SessionManager _s;

        private readonly ILogger<IndexModel> _logger;

        private AccountController _a;

        public LoginModel(ILogger<IndexModel> logger, SessionManager s, AccountController a)
        {
            _logger = logger;
            _s = s;
            _a = a;
        }

        public void OnGet()
        {

        }

        public async void OnPost()
        {
            int sid = _s.SID();
            bool isusernameemail = Regex.IsMatch(Username, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            Username = isusernameemail == true ? SQL.Accounts.GetUsername(Username) : Username;
            IActionResult result = await _a.Login(Username, Password);
            if (result is OkObjectResult)
            {
                Result = "Login successful. Logged in as: " + Username + ".";
            }
            else if (result is BadRequestObjectResult)
            {
                Result = "Invalid login";
            }
            else if (result is StatusCodeResult)
            {
                Result = "An error occurred while logging in";
            }
            else
            {
                Result = "Either username or password is blank, or you're already logged in.";
            }
            TempData["Result"] = Result;
        }

        public async Task<IActionResult> OnPostGoogleLoginAsync()
        {
            var redirect = Url.Page("https://localhost:7081/login?method=google"); 
            var properties = new AuthenticationProperties { RedirectUri = redirect };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
    }
}