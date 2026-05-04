using System.ComponentModel.DataAnnotations;

namespace SertecDashboard.Api.Models;

public class MachinePart
{

    public int MachineId { get; set; }
    public int PartId { get; set; }

    public virtual Machine Machine { get; set; } = null!;
    public virtual Parts Part { get; set; } = null!;
}
