using Microsoft.AspNetCore.Mvc;

namespace LearningASPNETAndRazor
{
    [ApiController]
    [Route("api/[controller]")]
    public class MiscController : ControllerBase
    {
        [HttpGet("GeneratePassword")]
        public IActionResult GeneratePassword()
        {
            Console.WriteLine("GENERATING A FUCKING PASSWORD");
            string pass = Misc.GeneratePassword();
            return new JsonResult(new { Password = pass });
        }
    }
}
