using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using FunWebsiteThing.Controllers.Classes;

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
        // This method accesses the data from the Google OAuth2 login with the token we received from Google.
        // Additionally, based on the data, we either login the user or register them.
        public async Task<IActionResult> OnGetAsync()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme); // We validate the authentication token and make sure it was sent by Google
            if (result.Succeeded) // if it was successful, all the data stored in result is valid and we can use it
            {
                // Claims are the data that Google sends back to us
                // These can consist of the user's email, username, etc.
                var claims = result.Principal.Claims;
                var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value; // get username
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value; // get email

                // We login / register the user based on email
                if (!SQL.Accounts.DoesUserExist(email, "email")) 
                {
                    // to prevent conflicts if the username already exists, we check to see if the username exists
                    if (SQL.Accounts.DoesUserExist(username, "username"))
                    {
                        Random random = new Random();
                        int num = random.Next(1, 9999);
                        username = username + num.ToString();
                    }
                    string password = Password.GeneratePassword(); // generate a password
                    TempData["TempPassword"] = password; // we store this for WelcomeExternal's message
                    _a.Register(email, username, password, null, null); // register the user!
                }
                else
                {
                    _s.Login(username, SQL.Accounts.GetUserID(username), _s.SID()); // login!
                }

                // Redirect to WelcomeExternal (Index immediately if this isn't the user's first time)
                return RedirectToPage(Url.Page("/WelcomeExternal"));
            }

            Logger.Write("Google token validation failed!");
            // Redirect to Login and set Result to login failed.
            return RedirectToPage("/Login", new { Result = "Google login failed." });
        }
    }
}
