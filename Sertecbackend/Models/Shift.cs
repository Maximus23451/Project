using System.ComponentModel.DataAnnotations;

namespace SertecDashboard.Api.Models;

public class Shift
{
    [Key]
    [MaxLength(32)]
    public string Id { get; set; } = string.Empty;

    [MaxLength(50)]
    public string OperatorUsername { get; set; } = string.Empty;

    [MaxLength(100)]
    public string OperatorName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Role { get; set; } = string.Empty;


    public long StartTime { get; set; }


    [MaxLength(50)]
    public string StartTimeStr { get; set; } = string.Empty;

    public bool Active { get; set; } = true;


    public long? EndTime { get; set; }

    [MaxLength(50)]
    public string? EndTimeStr { get; set; }


    [MaxLength(50)]
    public string? EndedBy { get; set; }

    [MaxLength(2000)]
    public string? Report { get; set; }
}
