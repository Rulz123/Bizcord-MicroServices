using System.ComponentModel.DataAnnotations;

namespace ChannelManagementService.Models;
public class Channels
{
    [Key]
    public Guid id { get; set; }
    public Guid serverId { get; set; }
    public string categories { get; set; }
    public string type { get; set; }
    public string userroles { get; set; }
    public bool visibility { get; set; }
    public string writerights { get; set; }
    public string description { get; set; }
    public string title { get; set; }
}