using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class WelcomeExternalModel : PageModel
    {
        // This is a simple redirect page that exists solely to display a message to users who logged in via OAuth2 and were just registered and given generated passes.
        // Therefore this method will redirect anyone else to Index.
        public void OnGet()
        {
            if (TempData["TempPassword"] == null) // if temppassword is null, this is likely the user's 2nd+ time returning or we have a funny bunny trying to access pages they shouldn't, therefore nothing to display.
            {
                Response.Redirect("/Index");
            }
        }
    }
}
