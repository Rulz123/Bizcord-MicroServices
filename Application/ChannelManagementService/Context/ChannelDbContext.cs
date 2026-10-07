using ChannelManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace ChannelManagementService.Context;

public class ChannelDbContext : DbContext
{
    
    public ChannelDbContext(DbContextOptions<ChannelDbContext> options) : base(options)
    {
        DbSet<Channels> Channels = Set<Channels>();
        
    }

    public DbSet<Channels> Channels { get; set; }
}
