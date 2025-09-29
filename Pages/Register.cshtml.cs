using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
#pragma warning disable CS8618

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
        public string SecurityQuestion { get; set; }
        [BindProperty]
        public string Answer { get; set; }
        [BindProperty]
        public bool Checkbox { get; set; }

        [BindProperty]
        public string Result { get; set; }

        private readonly ILogger<IndexModel> _logger;
        private AccountController _a;
        public RegisterModel(ILogger<IndexModel> logger, AccountController a)
        {
            _logger = logger;
            _a = a;
        }

        public void OnGet()
        {
            if (HttpContext.Session.GetInt32("IsLoggedIn") == 1)
            {
                Response.Redirect("/Index");
            }
        }
        public async Task<IActionResult> OnPost()
        {
            if (string.IsNullOrEmpty(Email)) // if email doesn't match regex / is empty
            {
                Result = "Email is blank";
            }
            else if (!Regex.IsMatch(Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                Result += "Invalid Email";
            }
            else if (string.IsNullOrEmpty(Username)) // if username is empty
            {
                Result += "Username is blank";
            }
            else if (!Regex.IsMatch(Username, @"^(?!\s)(?!.*[\W_]{2,})[a-zA-Z0-9_\s]+$"))
            {
                Result += "Invalid Username";
            }
            else if (string.IsNullOrEmpty(Password)) // if password is empty
            {
                Result += "Password is blank";
            }
            else if (string.IsNullOrEmpty(SecurityQuestion)) // if security question / answer is empty
            {
                Result += "The security question or the answer to it cannot be blank!";
            }
            else if (string.IsNullOrEmpty(Answer))
            {
                Result += "The security question or the answer to it cannot be blank!";
            }
            else if (Checkbox)
            {
                IActionResult result = await _a.Register(Email, Username, Password, SecurityQuestion, Answer);
                if (result is OkObjectResult)
                {
                    Result = "Account Registered. Logged into " + Username + ".";
                }
                else if (result is BadRequestObjectResult)
                {
                    Result = "Duplicate Account.";
                }
                else if (result is StatusCodeResult scr && scr.StatusCode == 500)
                {
                    Result = "An error occured while registering the account.";
                }
                else if (result is StatusCodeResult scr2 && scr2.StatusCode == 403)
                {
                    Result = "You are already logged in.";
                }
            }
            else
            {
                Result = "You did not click the verification checkbox.";
            }
            TempData["Result"] = Result;
            Console.WriteLine(TempData["Result"]);
            return Page();
        }
    }
}