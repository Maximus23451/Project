using System.ComponentModel.DataAnnotations;

namespace SertecDashboard.Api.Models;

public class Question
{
    [Key]
    [MaxLength(32)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Text { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Freq { get; set; } = "Every 1 hour";

    [MaxLength(30)]
    public string Type { get; set; } = "production";

    [MaxLength(5)]
    public string AlertAnswer { get; set; } = "no";

    [MaxLength(50)]
    public string YesLabel { get; set; } = "Igen";

    [MaxLength(50)]
    public string NoLabel { get; set; } = "Nem";

    [MaxLength(10)]
    public string RequireExplanation { get; set; } = "no";

    public long AnswerWindowMs { get; set; } = 600_000;

    [MaxLength(50)]
    public string CreatedAt { get; set; } = string.Empty;


    public long LastSent { get; set; } = 0;

    public long LastShiftSentMs { get; set; } = 0;
}
