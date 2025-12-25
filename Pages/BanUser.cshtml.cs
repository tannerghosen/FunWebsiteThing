using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

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

        private readonly SessionManager _s;

        public BanUserModel(SessionManager s)
        {
            _s = s;
        }
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
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                if (Id == _s.GetSession().UserId)
                {
                    Logger.Write("User tried to ban themself, ignoring ban user request.");
                    TempData["Result"] = "You cannot ban yourself.";
                }
                else
                {
                    if (string.IsNullOrEmpty(Reason)) Reason = "You have been banned.";
                    if (ExpirationDate == DateTime.MinValue) ExpirationDate = DateTime.Now.AddMonths(1);
                    await SQL.Admin.BanUser(Id, Reason, ExpirationDate);
                    TempData["Result"] = "Banned UserId " + Id + ".";
                }
            }

            return Page();
        }
    }
}
