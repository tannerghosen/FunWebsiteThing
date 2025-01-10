using Microsoft.AspNetCore.Mvc;

// This is an API controller that connects with the Misc class to call the method GeneratePassword and return it as a json string.
namespace FunWebsiteThing
{
    // Route for controller APIs is /api/Controller/Action
    // example: To access GeneratePassword, route would be /api/Misc/GeneratePassword
    [ApiController]
    [Route("api/[controller]")]
    public class MiscController : ControllerBase
    {
        [HttpGet("GeneratePassword")]
        public IActionResult GeneratePassword()
        {
            string pass = Misc.GeneratePassword();
            return new JsonResult(new { Password = pass });
        }
    }
}
