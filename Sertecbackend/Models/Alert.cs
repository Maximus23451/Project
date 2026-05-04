using System.ComponentModel.DataAnnotations;

namespace SertecDashboard.Api.Models;

public class Alert
{
    [Key]
    [MaxLength(32)]
    public string Id { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Type { get; set; } = "missed_question";

    [MaxLength(50)]
    public string OperatorUsername { get; set; } = string.Empty;

    [MaxLength(100)]
    public string OperatorName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;


    [MaxLength(32)]
    public string? PendingId { get; set; }


    [MaxLength(50)]
    public string Time { get; set; } = string.Empty;


    public long TimeMs { get; set; }

    public bool Acknowledged { get; set; } = false;

    [MaxLength(32)]
    public string? ShiftId { get; set; }
}
