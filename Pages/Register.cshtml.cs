using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace FunWebsiteThing.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public bool Checkbox { get; set; }

        [BindProperty]
        public string Result { get; set; }

        private SQLStuff _sq = new SQLStuff();

        private readonly ILogger<IndexModel> _logger;
        private SessionController _s;

        public RegisterModel(ILogger<IndexModel> logger, SessionController s, SQLStuff sq)
        {
            _logger = logger;
            _s = s;
            _sq = sq;
        }

        public void OnGet()
        {

        }
        public void OnPost()
        {
            int sid = _s.SID();
            if (string.IsNullOrEmpty(Email)) // if email doesn't match regex / is empty
            {
                Result = "Email is blank";
            }
            else if (!Regex.IsMatch(Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                Result = "Invalid Email";
            }
            else if (string.IsNullOrEmpty(Username)) // if username is empty
            {
                Result = "Username is blank";
            }
            else if (string.IsNullOrEmpty(Password)) // if password is empty
            {
                Result = "Password is blank";
            }
            else if (Checkbox)
            {
                if (HttpContext.Session.GetInt32("IsLoggedIn") != 1) // If we get a result, return the results
                {
                    (bool result, bool error) = _sq.Register(Email, Username, Password, sid); // Registers our account, hopefully
                    if (result == true)
                    {
                        /* To do, add verification? */
                        Result = "Account Registered. Logged into " + Username + ".";
                        _s.Login(Username, _sq.GetUserID(Username), sid);
                        Console.WriteLine(HttpContext.Session.GetString("Username") + " " + HttpContext.Session.GetInt32("UserId") + " " + HttpContext.Session.GetInt32("SessionId") + " " + HttpContext.Session.GetInt32("IsLoggedIn"));
                    }
                    else if (result == false && error != true)
                    {
                        Result = "Duplicate account.";
                    }
                    else if (error == true)
                    {
                        Result = "An error occured while registering the account.";
                    }
                }
            }
            else
            {
                Result = "You did not verify your stuff.";
            }
            TempData["Result"] = Result;
            Console.WriteLine(TempData["Result"]);
        }
    }
}