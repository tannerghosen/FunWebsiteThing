using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace FunWebsiteThing.Pages
{
    public class ForgetPasswordModel : PageModel
    {
        SessionManager _s = new SessionManager();
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Result { get; set; }
        public void OnGet()
        {
        }
        public void OnPost()
        {
            bool isusernameemail = Regex.IsMatch(Username, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            Username = isusernameemail == true ? SQL.Accounts.GetUsername(Username) : Username;
            if (SQL.Accounts.DoesUserExist(Username))
            {
                int id = SQL.Accounts.GetUserID(Username);
                TempData["Id"] = id; // This is the TempData Id we use to assign Id in both SecurityQuestion and ChangePassword to, which we pass along!
                Response.Redirect("/SecurityQuestion");
            }
            else
            {
                Result = "User does not exist with either email or username used!";
            }
        }
    }
}
