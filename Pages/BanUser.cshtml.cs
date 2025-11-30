using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
#pragma warning disable CS8618

namespace FunWebsiteThing.Pages
{
    public class BanUserModel : PageModel
    {
        [BindProperty]
        public int Id { get; set; }
        [BindProperty]
        public string Reason { get; set; }
        [BindProperty]
        public DateTime ExpirationDate { get; set; }

        [BindProperty]
        public string Result { get; set; }

        public void OnGet()
        {
            if (HttpContext.Session.GetInt32("IsAdmin") != 1 && !SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                Response.Redirect("/Index");
            }
            Id = Convert.ToInt32(Request.Query["userid"]);
        }

        public async Task<IActionResult> OnPost()
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                if (string.IsNullOrEmpty(Reason)) Reason = "You have been banned.";
                if (ExpirationDate == DateTime.MinValue) ExpirationDate = DateTime.Now.AddMonths(1);
                SQL.Admin.BanUser(Id, Reason, ExpirationDate);
            }
            TempData["Result"] = "Banned UserId " + Id + ".";

            return Page();
        }
    }
}
