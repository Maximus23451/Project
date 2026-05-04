using System.ComponentModel.DataAnnotations;

namespace SertecDashboard.Api.Models;


public class Document
{
    [Key]
    [MaxLength(32)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;


    public string Size { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;

    [MaxLength(50)]
    public string UploadedAt { get; set; } = string.Empty;
}
