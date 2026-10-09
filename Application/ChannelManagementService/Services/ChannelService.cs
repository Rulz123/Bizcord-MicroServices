using ChannelManagementService.Context;
using ChannelManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace ChannelManagementService.Services;

public class ChannelService
{
    private readonly ChannelDbContext _context;

    public ChannelService(ChannelDbContext context)
    {
        _context = context;
    }

    public async Task<List<Channels>> GetChannelsAsync()
    {
        return await _context.Channels
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Channels?> GetChannelByIdAsync(Guid id)
    {
        return await _context.Channels
            .AsNoTracking()
            .FirstOrDefaultAsync(channel => channel.id == id);
    }

    public async Task<Channels> CreateChannelAsync(CreateChannelRequest request)
    {
        var channel = new Channels
        {
            id = Guid.NewGuid(),
            serverId = request.ServerId,
            title = request.Title.Trim(),
            type = request.Type,
            categories = request.Categories,
            userroles = request.UserRoles,
            visibility = request.Visibility,
            writerights = request.WriteRights,
            description = request.Description
        };

        _context.Channels.Add(channel);
        await _context.SaveChangesAsync();

        return channel;
    }

    public async Task<bool> UpdateChannelAsync(Guid id, UpdateChannelRequest request)
    {
        var channel = await _context.Channels
            .FirstOrDefaultAsync(channel => channel.id == id);

        if (channel is null)
        {
            return false;
        }

        channel.title = request.Title.Trim();
        channel.type = request.Type;
        channel.categories = request.Categories;
        channel.userroles = request.UserRoles;
        channel.visibility = request.Visibility;
        channel.writerights = request.WriteRights;
        channel.description = request.Description;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteChannelAsync(Guid id)
    {
        var channel = await _context.Channels
            .FirstOrDefaultAsync(channel => channel.id == id);

        if (channel is null)
        {
            return false;
        }

        _context.Channels.Remove(channel);
        await _context.SaveChangesAsync();

        return true;
    }

}