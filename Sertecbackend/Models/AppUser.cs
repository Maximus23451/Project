using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SertecDashboard.Api.Models;

public class AppUser
{
    [Key]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; }

    [Required]
    [ForeignKey("Roles")]
    public int Role { get; set; }


    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;


    [MaxLength(200)]
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? RFID { get; set; }

    public virtual Roles Roles { get; set; }
}
