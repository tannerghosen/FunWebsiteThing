using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Principal;

namespace LearningASPNETAndRazor.Pages
{
    public class SandboxModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public SandboxModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}