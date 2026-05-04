using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SertecDashboard.Api.Models;

public class PasswordReset
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;


    [Column("Role")]
    public int RoleId { get; set; }

    [MaxLength(50)]
    public string RequestedAt { get; set; } = string.Empty;

    public long RequestedAtMs { get; set; }

    public bool Handled { get; set; } = false;

    [MaxLength(50)]
    public string? HandledAt { get; set; }


    [ForeignKey("RoleId")]
    public virtual Roles Roles { get; set; }
}
