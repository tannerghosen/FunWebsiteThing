using FunWebsiteThing.Controllers.Classes;
using Microsoft.AspNetCore.Mvc;

// This is an API controller that connects with the Misc class to call the method GeneratePassword and return it as a json string.
namespace FunWebsiteThing.Controllers
{
    public class PasswordModel
    {
        public string Password { get; set; }
    }
    // Route for controller APIs is /api/Controller/Action
    // example: To access GeneratePassword, route would be /api/Password/Generate because the Controller part of the class name is omitted, and Generate is the GET
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

        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            DateTime time = DateTime.Now;
            TimeSpan ping = DateTime.Now - time;
            return new JsonResult(new { Ping = ping.TotalMilliseconds + "ms" });
        }
    }
}
