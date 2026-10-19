using ChannelManagementService.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChannelManagementService.Controllers;

[ApiController]
[Route("[controller]")]
public class ChannelManagementController : ControllerBase
{
    private ChannelDbContext _context;

    public ChannelManagementController(ChannelDbContext context)
    {
        _context = context;

        
      
    }     
}
