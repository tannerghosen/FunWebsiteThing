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
    public class HandleGoogleLoginModel : PageModel
    {
        private readonly SessionManager _s;
        private readonly AccountController _a;
        private IHttpContextAccessor _h;

        public HandleGoogleLoginModel(SessionManager s, AccountController a, IHttpContextAccessor h)
        {
            _s = s;
            _a = a;
            _h = h;
        }
        // This method accesses the processed response the middleware at signin-google gets from the challenge we started back in Login.cshtml.cs.
        // Based on the data, we either login the user or register them, or we redirect back to login because the authentication in middleware failed.
        // The next step is to redirect to WelcomeExternal, which either is the end of the process or it redirects to Index and stops there
        // HandleGoogleLogin -> WelcomeExternal (redirect, all log in related stuff is finalized here)
        public async Task<IActionResult> OnGetAsync()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme); // Authenticate the request using Google's auth scheme
            if (result.Succeeded)
            {
                // Claims are the data that Google sends back to us that contains the authenticated user's identity
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

                    // This is a work around as HttpContext for some reason is initially null/uninitialized during the first time register/login via AccountController, so we just handle it by logging in (making a session) via SessionManager here instead of AccountController's Register method calling it.
                    _a.Register(email, username, password, "", "", true);
                    _s.Login(username, FunWebsiteThing.SQL.Accounts.GetUserID(username), 0);
                    _s.FWTCookie(username, FunWebsiteThing.SQL.Accounts.GetUserID(username), 0, true, FunWebsiteThing.SQL.Admin.IsAdmin(FunWebsiteThing.SQL.Accounts.GetUserID(username)));
                }
                else
                {
                    _a.Login(email, "", true);
                    _s.FWTCookie(username, FunWebsiteThing.SQL.Accounts.GetUserID(username), 0, true, FunWebsiteThing.SQL.Admin.IsAdmin(FunWebsiteThing.SQL.Accounts.GetUserID(username)));
                }

                return RedirectToPage(Url.Page("/WelcomeExternal"));
            }
            else
            {
                Logger.Write("Google authentication failed!", "ERROR");
                return RedirectToPage("/Login", new { Result = "Google authentication failed." });
            }
        }
    }
}
