using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class IPBanModel : PageModel
    {
        [BindProperty]
        public string IP { get; set; }
        [BindProperty]
        public string Reason { get; set; }
        [BindProperty]
        public DateTime ExpirationDate { get; set; }
        [BindProperty]
        public bool Checkbox { get; set; }
        [BindProperty]
        public string Result { get; set; }

        private readonly SessionManager _s;

        public IPBanModel(SessionManager s)
        {
            _s = s;
        }

        public void OnGet()
        {
            if (HttpContext.Session.GetInt32("IsAdmin") != 1 && !SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                Response.Redirect("/Index");
            }
        }

        public async Task<IActionResult> OnPost()
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && SQL.Admin.IsAdmin(HttpContext.Session.GetInt32("UserId")))
            {
                if (!string.IsNullOrEmpty(IP))
                {
                    if (Checkbox)
                    {
                        if (_s.GetIP() == IP)
                        {
                            Logger.Write("IP matches session's, disregarding IP ban request.");
                            TempData["Result"] = "You cannot ban your own IP.";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(Reason)) Reason = "You have been banned.";
                            if (ExpirationDate == DateTime.MinValue) ExpirationDate = DateTime.Now.AddMonths(1);
                            await SQL.Admin.BanIP(IP, Reason, ExpirationDate);
                            TempData["Result"] = "Banned " + IP + ".";
                        }
                    }
                    else if (!Checkbox)
                    {
                        await SQL.Admin.UnbanIP(IP);
                        TempData["Result"] = "Unbanned " + IP + ".";
                    }
                }
                else if (string.IsNullOrEmpty(IP)) 
                {
                }
            }
            TempData["Result"] = Result;

            return Page();
        }
    }
}
