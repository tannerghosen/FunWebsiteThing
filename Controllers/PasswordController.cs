using FunWebsiteThing.Controllers.Classes;
using Microsoft.AspNetCore.Mvc;

// This is an API controller that connects with the Misc class to call the method GeneratePassword and return it as a json string.
namespace FunWebsiteThing.Controllers
{
    public class PasswordModel
    {
        public string Password { get; set; }
    }
    
    // Routing is pretty simple, api/[controller] is api/Password as the latter part is omitted. So getting Generate would be api/Password/Generate.
    // If you wanted it to simply be /(Action), you'd blank the Route string. Simply api/(Action), make Route "api/", etc.
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordController : ControllerBase
    {
        [HttpGet("Generate")]
        public IActionResult GeneratePassword()
        {
            string pass = Password.GeneratePassword();
            return new JsonResult(new { Password = pass });
        }

        [HttpPost("Validate")]
        public IActionResult ValidatePassword([FromBody] PasswordModel password)
        {
            bool strength = Password.ValidatePassword(password.Password);
            return new JsonResult(new { Strong = strength });
        }
    }
}
