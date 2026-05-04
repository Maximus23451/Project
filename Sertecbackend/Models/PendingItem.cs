using System.ComponentModel.DataAnnotations;

namespace SertecDashboard.Api.Models;

public class PendingItem
{
    [Key]
    [MaxLength(32)]
    public string Id { get; set; } = string.Empty;

    [MaxLength(32)]
    public string QuestionId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Text { get; set; } = string.Empty;

    [MaxLength(50)]
    public string SentAt { get; set; } = string.Empty;


    public long SentAtMs { get; set; }


    public long Deadline { get; set; }

    public long AnswerWindowMs { get; set; } = 600_000;

    [MaxLength(5)]
    public string AlertAnswer { get; set; } = "no";

    [MaxLength(50)]
    public string? TargetOperator { get; set; }

    [MaxLength(100)]
    public string? TargetOperatorName { get; set; }

    [MaxLength(50)]
    public string YesLabel { get; set; } = "Igen";

    [MaxLength(50)]
    public string NoLabel { get; set; } = "Nem";

    [MaxLength(10)]
    public string RequireExplanation { get; set; } = "no";

    [MaxLength(10)]
    public string SentBy { get; set; } = "auto";

    [MaxLength(32)]
    public string? ShiftId { get; set; }

    public bool AlertSent { get; set; } = false;

    public bool Expired { get; set; } = false;
}
