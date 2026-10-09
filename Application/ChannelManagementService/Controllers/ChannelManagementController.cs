using ChannelManagementService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChannelManagementService.Controllers;

[ApiController]
[Route("api/channels")]
public class ChannelManagementController : ControllerBase
{
    private readonly ChannelService _channelService;

    public ChannelManagementController(ChannelService channelService)
    {
        _channelService = channelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetChannels()
    {
        var channels = await _channelService.GetChannelsAsync();

        return Ok(channels);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetChannelById(Guid id)
    {
        var channel = await _channelService.GetChannelByIdAsync(id);

        if (channel is null)
        {
            return NotFound();
        }

        return Ok(channel);
    }
}