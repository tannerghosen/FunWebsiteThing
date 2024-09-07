using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Principal;

namespace LearningASPNETAndRazor.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public RegisterModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}