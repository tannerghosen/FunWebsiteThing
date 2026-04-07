using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace FunWebsiteThing.Pages
{
    [DisableRateLimiting] // This allows this page to be actually accessible during a rate limiting scenario, which is ideal as it tells you you're being ratelimited.
    public class ErrorModel : PageModel
    {
        [BindProperty]
        public int? ErrorCode { get; set; }
        [BindProperty]
        public string? ErrorMessage { get; set; }
        public void OnGet()
        {
            ErrorCode = Request.Query.ContainsKey("error") ? Convert.ToInt32(Request.Query["error"]) : 0;
            switch (ErrorCode)
            {
                case 0:
                default:
                    ErrorMessage = "An unknown server error has occured.";
                    break;
                case 404:
                    ErrorMessage = "This page does not exist.";
                    break;
                case 429:
                    ErrorMessage = "You are being rate limited by the server.\nIf you are the owner of a web crawler / bot, you are limited to 20 requests per minute.\nAttempts to step around this will get your web crawlers / bots banned.";
                    break;
            }
        }
    }
}
