using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
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
            (bool b, int? id, string? reason, DateTime? expire) = SQL.Admin.IsUserBanned(SQL.Accounts.GetUserID(Username));
            if ((Username == null || Username == "") || (Password == null || Password == ""))
            {
                Result = "Username or Password is blank.";
                return Page();
            }
            IActionResult result = await _a.Login(Username, Password);
            if (result is OkObjectResult && b)
            {
                DateTime permacheck = DateTime.Now.AddYears(100);
                Result = expire <= permacheck ? "Your account is banned until " + expire + ".\\nReason: " + reason + "\\nIf you believe this to be in error, contact the admins." : "Your account is permanently banned.\\nReason: " + reason + "\\nIf you believe this to be in error, contact the admins.";
            }
            else if (result is OkObjectResult)
            {
                Result = "Login successful. Logged in as: " + Username + ".";
            }
            else if (result is BadRequestObjectResult)
            {
                Result = "Invalid login.";
            }
            else if (result is StatusCodeResult scr && scr.StatusCode == 500)
            {
                Result = "An error occurred while logging in.";
            }
            else if (result is StatusCodeResult scr2 && scr2.StatusCode == 403)
            {
                Result = "You are already logged in.";
            }
            TempData["Result"] = Result;

            return Page();
        }

        // This method initiates the OAuth2 authentication flow with Google by creating a challenge
        // The next step is middleware processing the response at signin-google, followed by registering / logging in being handled in HandleGoogleLogin
        // Login -> Middleware (signin-google) -> HandleGoogleLogin -> WelcomeExternal
        public IActionResult OnPostGoogleLoginAsync()
        {
            TempData["LoginSource"] = "Google";
            var redirect = Url.Page("/HandleGoogleLogin");  // this is our page which contains the code for handling this request
            var properties = new AuthenticationProperties { RedirectUri = redirect }; // set the redirect as the redirect uri
            return Challenge(properties, GoogleDefaults.AuthenticationScheme); // create a challenge that makes the user grant access to their Google account for their user identity.
        }
    }
}