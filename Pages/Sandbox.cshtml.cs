using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunWebsiteThing.Pages
{
    public class SandboxModel : PageModel
    {
        public string? Name { get; private set; }
        public string? Color { get; private set; }

        private readonly ILogger<IndexModel> _logger;

        public SandboxModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        // URL Parameter handling, Model side
        public void OnGet()
        {
            Name = Request.Query["name"];
            Color = Request.Query["favcolor"];

            Console.WriteLine("Name: " + Name + " " + "Color: " + Color);
        }
        public void OnPost()
        {
            Name = Request.Query["name"];
            Color = Request.Query["favcolor"];
            Console.WriteLine("Name: " + Name + " " + "Color: " + Color);
        }
    }
}