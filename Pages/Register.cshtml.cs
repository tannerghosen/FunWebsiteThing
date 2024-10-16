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

        private SQLStuff _s = new SQLStuff();

        private readonly ILogger<IndexModel> _logger;

        public RegisterModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
        public void OnPost()
        {
            Console.WriteLine(Checkbox);
            Random r = new Random();
            int sid = r.Next(999999999);
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
                Result = "Username is blank";
            }
            else if (Checkbox)
            {
                if (HttpContext.Session.GetInt32("IsLoggedIn") != 1) // If we get a result, return the results
                {
                    Console.WriteLine("hello");
                    (bool result, bool error) = _s.Register(Email, Username, Password, sid); // Registers our account, hopefully
                    if (result == true)
                    {
                        Result = "Account Registered. Logged into " + Username + ".";
                        HttpContext.Session.SetString("Username", Username);
                        HttpContext.Session.SetInt32("UserId", _s.GetUserID(Username));
                        HttpContext.Session.SetInt32("SessionId", sid);
                        HttpContext.Session.SetInt32("IsLoggedIn", 1);
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