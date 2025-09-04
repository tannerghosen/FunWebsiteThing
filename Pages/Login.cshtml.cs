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
            if (HttpContext.Session.GetInt32("IsLoggedIn") == 1)
            {
                Response.Redirect("/Index");
            }
        }

        // Changed from void to IActionResult because void doesn't actually wait for methods. For some reason, this was not an issue before we switched to MySQL, funny enough.
        public async Task<IActionResult> OnPost()
        {
            if ((Username == null || Username == "") || (Password == null || Password == ""))
            {
                Result = "Username or Password is blank.";
                return Page();
            }
            IActionResult result = await _a.Login(Username, Password);
            if (result is OkObjectResult)
            {
                Result = "Login successful. Logged in as: " + Username + ".";
            }
            else if (result is BadRequestObjectResult)
            {
                Result = "Invalid login";
            }
            else if (result is StatusCodeResult scr && scr.StatusCode == 500)
            {
                Result = "An error occurred while logging in";
            }
            else if (result is StatusCodeResult scr2 && scr2.StatusCode == 409)
            {
                Result = "You are already logged in.";
            }
            TempData["Result"] = Result;

            return Page();
        }

        // This method initiates the OAuth2 authentication flow with Google by creating a challenge
        // The next step (handling the response to the challenge) is handled in SigninGoogle.cshtml.cs
        // Login -> SigninGoogle (SigninGoogle proceses the response to the challenge, finalizes login)
        public async Task<IActionResult> OnPostGoogleLoginAsync()
        {
            TempData["LoginSource"] = "Google";
            var redirect = Url.Page("/SignInGoogle");  // this is our page which contains our callback method
            var properties = new AuthenticationProperties { RedirectUri = redirect }; // set the properties for the login challenge
            return Challenge(properties, GoogleDefaults.AuthenticationScheme); // create a challenge the makes the user grant access to their Google account for the OAuth2 flow, along with a redirect after that.
        }
    }
}