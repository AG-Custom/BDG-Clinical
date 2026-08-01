using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.Health;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "AG.CLINICAL.WebApi"
        });
    }
}
