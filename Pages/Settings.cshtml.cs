using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace LearningASPNETAndRazor.Pages
{
    public class SettingsModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string Result { get; set; }

        private SQLStuff _s = new SQLStuff();
        public void OnGet()
        {
        }

        public void OnPost()
        {
            if (HttpContext.Session.GetInt32("IsLoggedIn") == 1 && HttpContext.Session.GetString("Username") != null && _s.DoesUserExist(HttpContext.Session.GetString("Username")) == true)
            {
                if (!string.IsNullOrEmpty(Password))
                {
                    bool passwordupdated = _s.UpdateInfo(HttpContext.Session.GetString("Username"), 0, Password);
                    if (passwordupdated)
                    {
                        Result = "Password has been changed. Be sure to write it down or save it in your browser!";
                    }
                    else
                    {
                        Result = "Unknown error";
                    }
                }
                if (!string.IsNullOrEmpty(Email) && Regex.IsMatch(Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    bool emailupdated = _s.UpdateInfo(HttpContext.Session.GetString("Username"), 1, Email);
                    if (emailupdated)
                    {
                        Result = "Email has been updated to " + Email;
                    }
                    else
                    {
                        Result = "Email is already in use by another account!";
                    }
                }
                if (!string.IsNullOrEmpty(Username))
                {
                    bool usernameupdated = _s.UpdateInfo(HttpContext.Session.GetString("Username"), 2, Username);
                    if (usernameupdated)
                    {
                        HttpContext.Session.SetString("Username", Username);
                        Result = "Username updated to " + Username;
                    }
                    else
                    {
                        Result = "Username is already in use by another account!";
                    }
                }
            }
        }
    }
}
