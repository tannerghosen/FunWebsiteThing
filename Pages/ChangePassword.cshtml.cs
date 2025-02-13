using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class ChangePasswordModel : PageModel
    {
        SessionManager _s;
        [BindProperty]
        public string Id { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string Result { get; set; }

        public void OnGet()
        {
            Id = TempData["Id"]?.ToString(); // get tempdata Id from securityquestion which comes from forgetpassword
        }

        public async Task<IActionResult> OnPost()
        {
            Id = Request.Form["Id"];
            if (!string.IsNullOrEmpty(Password))
            {
                int id = int.TryParse(Id, out int newid) ? newid : 0;
                SQL.Accounts.UpdateInfo(id, 0, Password, 0, true); 
                Result = "Password has been changed.";
                TempData["Id"] = null; // to prevent abuse, we set tempdata id to null
                return Redirect("/Login");
            }
            else
            {
                Result = "Invalid password";
                TempData["Id"] = Id; // ensure tempdata is still set to id
                return Redirect("/ChangePassword");
            }
        }

    }
}
