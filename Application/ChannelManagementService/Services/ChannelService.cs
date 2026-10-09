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

}