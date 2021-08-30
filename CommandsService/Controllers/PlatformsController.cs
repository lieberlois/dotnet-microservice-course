using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [Route("api/c/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        [HttpPost]
        public ActionResult TestInboundConn()
        {
            System.Console.WriteLine("Serving Inbound Connection");
            return Ok();
        }
    }
}