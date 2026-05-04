using System.ComponentModel.DataAnnotations;

namespace SertecDashboard.Api.Models;

public class Machine
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public List<MachinePart> MachineParts { get; set; } = new();
}
