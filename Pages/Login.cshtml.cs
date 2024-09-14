using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Principal;

namespace LearningASPNETAndRazor.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string Result { get; set; }

        private SQLStuff _s = new SQLStuff();

        private readonly ILogger<IndexModel> _logger;

        public LoginModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public void OnPost()
        {
            bool result = _s.Login(Username, Password);
            if ((!string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password)) && HttpContext.Session.GetInt32("IsLoggedIn") != 1)
            {
                if (result == true)
                {
                    HttpContext.Session.SetString("Username", Username);
                    HttpContext.Session.SetInt32("IsLoggedIn", 1);
                    Result = "Login successful. Logged in as: " + Username + ".";
                }
                else
                {
                    Result = "Invalid login";
                }
            }
        }
    }
}