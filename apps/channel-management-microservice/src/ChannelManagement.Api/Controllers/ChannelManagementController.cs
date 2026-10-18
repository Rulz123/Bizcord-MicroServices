using ChannelManagementService.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChannelManagementService.Controllers;

[ApiController]
[Route("[controller]")]
public class ChannelManagementController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ChannelDbContext _context;

    public ChannelManagementController()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();

        var optionsBuilder = new DbContextOptionsBuilder<ChannelDbContext>();
        optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));

        _context = new ChannelDbContext(optionsBuilder.Options);
    }
}
