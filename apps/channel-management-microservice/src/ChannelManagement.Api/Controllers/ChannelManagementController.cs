using ChannelManagement.Api.Services;
using ChannelManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChannelManagement.Api.Controllers;

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

    [HttpPost]
    public async Task<IActionResult> CreateChannel([FromBody] CreateChannelRequest request)
    {
        if (request.ServerId == Guid.Empty)
        {
            ModelState.AddModelError(
                nameof(request.ServerId),
                "ServerId must be a non-empty GUID.");

            return ValidationProblem(ModelState);
        }

        var channel = await _channelService.CreateChannelAsync(request);

        return CreatedAtAction(
            nameof(GetChannelById),
            new { id = channel.id },
            channel);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateChannel(Guid id, [FromBody] UpdateChannelRequest request)
    {
        var updated = await _channelService.UpdateChannelAsync(id, request);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteChannel(Guid id)
    {
        var deleted = await _channelService.DeleteChannelAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}