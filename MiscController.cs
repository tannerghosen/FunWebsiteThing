using Microsoft.AspNetCore.Mvc;

// This is an API controller that connects with the Misc class to call the method GeneratePassword and return it as a json string.
namespace LearningASPNETAndRazor
{
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
