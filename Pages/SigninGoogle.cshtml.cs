using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using FunWebsiteThing.Controllers.Classes;
using System.Configuration;
using MySqlX.XDevAPI;

namespace FunWebsiteThing.Pages
{
    // This is the callback page after we initiated the OAuth2 login challenge in Login. We redirect to WelcomeExternal.
    public class SigninGoogleModel : PageModel
    {
        private readonly SessionManager _s;
        private readonly AccountController _a;
        private IHttpContextAccessor _h;

        public SigninGoogleModel(SessionManager s, AccountController a, IHttpContextAccessor h)
        {
            _s = s;
            _a = a;
            _h = h;
        }
        // This method accesses the data from the Google OAuth2 login challenge with the token we received from Google.
        // Additionally, based on the data, we either login the user or register them.
        // The next step is to redirect to WelcomeExternal, which either is the end of the process or it redirects to Index and stops there
        public async Task<IActionResult> OnGetAsync()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme); // We validate the authentication token and make sure it was sent by Google
            if (result.Succeeded) 
            {
                // Claims are the data that Google sends back to us
                // These can consist of the user's email, username, etc.
                var claims = result.Principal.Claims;
                var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value; // Username
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value; // Email

                // We login / register the user based on email
                if (!SQL.Accounts.DoesUserExist(email, "email")) 
                {
                    if (SQL.Accounts.DoesUserExist(username, "username"))
                    {
                        // If this user already exists, we prevent an issue by adding a number to the end of the username
                        Random random = new Random();
                        int num = random.Next(1, 9999);
                        username = username + num.ToString();
                    }
                    string password = Password.GeneratePassword(); 
                    TempData["TempPassword"] = password; // we store this for WelcomeExternal's message so the user can see their password
                    _a.Register(email, username, password, "", "", true);
                    // ugly work-around as the session is null when we register from an OAuth source.
                    _h.HttpContext.Session.SetString("Username", username);
                    _h.HttpContext.Session.SetInt32("UserId", FunWebsiteThing.SQL.Accounts.GetUserID(username));
                    _h.HttpContext.Session.SetInt32("SessionId", 0); // we'll just have no sid for registration
                    _h.HttpContext.Session.SetInt32("IsLoggedIn", 1);
                    _h.HttpContext.Session.SetInt32("IsAdmin", FunWebsiteThing.SQL.Admin.IsAdmin(_h.HttpContext.Session.GetInt32("UserId")) == true ? 1 : 0);
                    Logger.Write("Username: " + username + " id: " + FunWebsiteThing.SQL.Accounts.GetUserID(username) + " ses id: N/A (0)" + " is admin?: " + FunWebsiteThing.SQL.Admin.IsAdmin(_h.HttpContext.Session.GetInt32("UserId")), "GOOGLE FIRST-TIME LOGIN");
                }
                else
                {
                    _a.Login(email, "", true);
                }

                return RedirectToPage(Url.Page("/WelcomeExternal"));
            }

            Logger.Write("Google token validation failed!", "ERROR");
            return RedirectToPage("/Login", new { Result = "Google login failed." });
        }
    }
}
