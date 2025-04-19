using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class WelcomeExternalModel : PageModel
    {
        public void OnGet()
        {
            if (TempData["TempPassword"] == null)
            {
                Response.Redirect("/Index");
            }
        }
    }
}
