using FunWebsiteThing.SQL;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
using System.Security.Claims;
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

        // Changed from void to IActionResult because void doesn't actually wait for methods. For some reason, this was not an issue before we switched to MySQL, funny enough.
        public async Task<IActionResult> OnPost()
        {
            int sid = _s.SID();
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

            return Page();
        }

        // This method initiates the OAuth2 authentication flow with Google
        // By the time we get to redirect (SignInGoogle), we should have a token that can be validated and its data accessed.
        public async Task<IActionResult> OnPostGoogleLoginAsync()
        {
            TempData["LoginSource"] = "Google";
            var redirect = Url.Page("/SignInGoogle");  // this is our page which contains our callback method
            var properties = new AuthenticationProperties { RedirectUri = redirect }; // set the properties for the login challenge
            return Challenge(properties, GoogleDefaults.AuthenticationScheme); // create a challenge the makes the user grant access to their Google account for the OAuth2 flow, along with a redirect after that.
        }
    }
}