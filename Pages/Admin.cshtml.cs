using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class AdminModel : PageModel
    {

        private SQLStuff _sq;
        private SessionController _s;

        private readonly ILogger<IndexModel> _logger;

        public AdminModel(ILogger<IndexModel> logger, SQLStuff sq)
        {
            _logger = logger;
            _sq = sq;
        }
        public void OnGet()
        {
        }

        public IActionResult OnPostDelete(int? userid)
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1)
            {
                _sq.DeleteUser(userid);
            }

            return RedirectToPage();
        }
        public IActionResult OnPostAdmin(int? userid)
        {
            if (HttpContext.Session.GetInt32("IsAdmin") == 1 && HttpContext.Session.GetInt32("UserId") == 0)
            {
                _sq.AdminUser(userid);
            }

            return RedirectToPage();
        }

    }
}
