using ChannelManagementMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace ChannelManagementService.Context;

public class ChannelDbContext : DbContext
{
    private string? v;

    public ChannelDbContext(DbContextOptions<ChannelDbContext> options) : base(options)
    {
        DbSet<Channels> Channels = Set<Channels>();
        
    }

    public DbSet<Channels> Channels { get; set; }
}
