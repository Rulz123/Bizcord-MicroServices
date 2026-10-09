using System.ComponentModel.DataAnnotations;

namespace ChannelManagementService.Models;

public class UpdateChannelRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Voice|Text|Thread|Forums)$")]
    public string Type { get; set; } = string.Empty;

    public string Categories { get; set; } = string.Empty;
    public string UserRoles { get; set; } = string.Empty;
    public bool Visibility { get; set; }
    public string WriteRights { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}