using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public string? Username { get; set; }

        [BindProperty]
        public DateTime? JoinDate { get; set; }

        public void OnGet()
        {
            Id = Convert.ToInt32(Request.Query["UserId"]);
            Username = SQL.Accounts.GetUsername(Id) == null || SQL.Accounts.GetUsername(Id) == string.Empty ? "Not Registered" : SQL.Accounts.GetUsername(Id);
            JoinDate = SQL.Accounts.GetJoinDate(Id) == null || SQL.Accounts.GetJoinDate(Id) < DateTime.MinValue ? DateTime.MinValue : SQL.Accounts.GetJoinDate(Id);
        }
    }
}
