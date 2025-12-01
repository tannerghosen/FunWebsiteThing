using Microsoft.AspNetCore.Mvc;

namespace FunWebsiteThing.Controllers
{
    [ApiController]
    [Route("api/")]
    public class PingController : Controller
    {
        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            DateTime time = DateTime.Now;
            TimeSpan ping = DateTime.Now - time;
            return new JsonResult(new { Ping = ping.TotalMilliseconds + "ms" });
        }
    }
}
