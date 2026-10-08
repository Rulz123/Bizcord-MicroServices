using Microsoft.AspNetCore.Mvc;

namespace ChannelManagementService.Controllers;

[ApiController]
[Route("api/channels")]
public class ChannelManagementController : ControllerBase
{
    [HttpGet]
    public IActionResult GetChannels()
    {
        return Ok(new[] { "general", "announcements" });
    }
   
}
