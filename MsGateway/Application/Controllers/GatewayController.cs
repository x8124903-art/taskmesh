using Microsoft.AspNetCore.Mvc;

namespace MsGateway.Application.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/gateway")]
    public sealed class GatewayController : ControllerBase
    {
        [HttpGet("healthz")]
        public IActionResult Health()
        {
            return Ok(new { status = "ok" });
        }
    }
}