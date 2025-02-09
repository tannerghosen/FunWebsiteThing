using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FunWebsiteThing.Pages
{
    public class SigninGoogleModel : PageModel
    {
        private readonly SessionManager _s;
        private readonly AccountController _a;

        public SigninGoogleModel(SessionManager s, AccountController a)
        {
            _s = s;
            _a = a;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme); // Get the result of the Google OAuth2 login, including the claims if possible;
            if (result.Succeeded)
            {
                // Claims are the data that Google sends back to us
                // These can consist of the user's email, username, etc.
                // The user is either logged in or registered
                var claims = result.Principal.Claims;
                var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value; // get username
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value; // get email

                if (!SQL.Accounts.DoesUserExist(username)) 
                {
                    string password = Misc.GeneratePassword(); // generate a password
                    TempData["TempPassword"] = password; // we store this for WelcomeExternal's message
                    _a.Register(email, username, password, null, null); // register the user!
                }
                else
                {
                    _s.Login(username, SQL.Accounts.GetUserID(username), _s.SID()); // login!
                }

                // Redirect to WelcomeExternal (Index immediately if this isn't he user's first time)
                return RedirectToPage(Url.Page("/WelcomeExternal"));
            }

            Logger.Write("Google login failed!");
            // Redirect to Login and set Result to login failed.
            return RedirectToPage("/Login", new { Result = "Google login failed." });
        }
    }
}
