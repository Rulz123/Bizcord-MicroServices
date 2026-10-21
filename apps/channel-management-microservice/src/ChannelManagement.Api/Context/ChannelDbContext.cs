using ChannelManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ChannelManagement.Api.Context;

public class ChannelDbContext : DbContext
{
    public ChannelDbContext(DbContextOptions<ChannelDbContext> options)
        : base(options)
    {
    }

    public DbSet<Channels> Channels => Set<Channels>();
}
