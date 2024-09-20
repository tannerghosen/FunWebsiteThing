using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace FunWebsiteThing.Pages
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
            if (HttpContext.Session.GetInt32("IsLoggedIn") == 1 && HttpContext.Session.GetString("Username") != null && HttpContext.Session.GetInt32("UserId") != null && HttpContext.Session.GetInt32("SessionId") != null)
            {
                if (!string.IsNullOrEmpty(Password))
                {
                    bool passwordupdated = _s.UpdateInfo(HttpContext.Session.GetInt32("UserId"), 0, Password, HttpContext.Session.GetInt32("SessionId"));
                    if (passwordupdated)
                    {
                        Result += "Password has been changed. Be sure to write it down or save it in your browser!";
                    }
                    else
                    {
                        Result += "Unknown error";
                    }
                }
                if (!string.IsNullOrEmpty(Email) && Regex.IsMatch(Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    bool emailupdated = _s.UpdateInfo(HttpContext.Session.GetInt32("UserId"), 1, Email, HttpContext.Session.GetInt32("SessionId"));
                    if (emailupdated)
                    {
                        Result += "\\nEmail has been updated to " + Email;
                    }
                    else
                    {
                        Result += "\\nEmail is already in use by another account!";
                    }
                }
                if (!string.IsNullOrEmpty(Username))
                {
                    bool usernameupdated = _s.UpdateInfo(HttpContext.Session.GetInt32("UserId"), 2, Username, HttpContext.Session.GetInt32("SessionId"));
                    if (usernameupdated)
                    {
                        HttpContext.Session.SetString("Username", Username);
                        Result += "\\nUsername updated to " + Username;
                    }
                    else
                    {
                        Result += "\\nUsername is already in use by another account!";
                    }
                }
            }
            TempData["Result"] = Result;
            Console.WriteLine(TempData["Result"]);
        }
    }
}
