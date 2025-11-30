using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
#pragma warning disable CS8618

namespace FunWebsiteThing.Pages
{
    public class UpdateModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string Result { get; set; }

        [BindProperty]
        public int Id { get; set; }
        public void OnGet()
        {
            if (HttpContext.Session.GetInt32("IsAdmin") != 1 && !SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                Response.Redirect("/Index");
            }
            Id = Convert.ToInt32(Request.Query["Id"]);
        }

        public async Task<IActionResult> OnPost()
        {
            Id = Convert.ToInt32(Request.Form["Id"]);
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                if (!string.IsNullOrEmpty(Password))
                {
                    // for these if-elses with xupdated, the expected outcome is either it updates it or not.
                    // bool error is true if either a sql exception occurs, sql issues =/= bool error is true. ONLY if it causes an exception.
                                                                                   // id option value sid (n/a) isadmin
                    (bool passwordupdated, bool error) = await SQL.Accounts.UpdateInfo(Id, 0, Password, null, true); // isadmin is true as we are admin
                    if (passwordupdated)
                    {
                        Result += "Password has been changed."; 
                    }
                    else if (!passwordupdated && !error)
                    {
                        Result += "An unknown error occurred while changing the Password.";
                    }
                    else if (!passwordupdated && error)
                    {
                        Result += "An error occurred while changing the Password."; 
                    }
                }
                else if (string.IsNullOrEmpty(Password)) 
                {
                }

                if (!string.IsNullOrEmpty(Email) && Regex.IsMatch(Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    (bool emailupdated, bool error) = await SQL.Accounts.UpdateInfo(Id, 1, Email, null, true);
                    if (emailupdated)
                    {
                        Result += "\\nEmail has been updated to " + Email; 
                    }
                    else if (!emailupdated && !error)
                    {
                        Result += "\\nEmail is already in use by another account!"; 
                    }
                    else if (!emailupdated && error)
                    {
                        Result += "\\nAn error occurred while changing the Email."; 
                    }
                }
                else if (string.IsNullOrEmpty(Email)) 
                { 
                }
                else if (!Regex.IsMatch(Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    Result += "\\nEmail format is invalid. Emails should follow a format of name@emailprovider.com";
                }

                if (!string.IsNullOrEmpty(Username))
                {
                    (bool usernameupdated, bool error) = await SQL.Accounts.UpdateInfo(Id, 2, Username, null, true);
                    if (usernameupdated)
                    {
                        Result += "\\nUsername updated to " + Username;
                    }
                    else if (!usernameupdated && !error)
                    {
                        Result += "\\nUsername is already in use by another account!";
                    }
                    else if (!usernameupdated && error)
                    {
                        Result += "\\nAn error occurred while changing the Username."; 
                    }
                }
                else if (string.IsNullOrEmpty(Username))  // No need to show an error if it's empty
                {
                }
            }
            TempData["Result"] = Result;

            return Page();
        }
    }
}
