using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class WelcomeExternalModel : PageModel
    {
        public void OnGet()
        {
            if (TempData["TempPassword"] == null) // if temppassword is null, this is likely the user's 2nd+ time returning or we have a funny bunny trying to access pages they shouldn't, therefore nothing to display.
            {
                Response.Redirect("/Index");
            }
        }
    }
}
