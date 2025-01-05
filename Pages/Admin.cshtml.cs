using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class AdminModel : PageModel
    {

        private SQLStuff _sq;
        private SessionManager _s;

        private readonly ILogger<IndexModel> _logger;

        public AdminModel(ILogger<IndexModel> logger, SQLStuff sq)
        {
            _logger = logger;
            _sq = sq;
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostDelete(int? userid)
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1)
            {
                await _sq.DeleteUser(userid);
            }

            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostAdmin(int? userid)
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && HttpContext.Session.GetInt32("UserId") == 0)
            {
                await _sq.AdminUser(userid);
            }

            return RedirectToPage();
        }

    }
}
