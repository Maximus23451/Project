using System.ComponentModel.DataAnnotations;

namespace SertecDashboard.Api.Models;

public class QuestionResponse
{
    [Key]
    [MaxLength(32)]
    public string Id { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Question { get; set; } = string.Empty;


    [MaxLength(5)]
    public string Answer { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Operator { get; set; }

    [MaxLength(100)]
    public string OperatorName { get; set; } = "Operator";


    [MaxLength(5)]
    public string AlertAnswer { get; set; } = "no";


    [MaxLength(50)]
    public string Time { get; set; } = string.Empty;


    public long TimeMs { get; set; }

    [MaxLength(32)]
    public string? PendingId { get; set; }
}
