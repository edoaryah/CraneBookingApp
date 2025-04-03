// Models/UrgentLog.cs
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public class UrgentLog
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public int CraneId { get; set; }

    [Required]
    public DateTime UrgentStartTime { get; set; } = DateTime.Now;

    [Required]
    public int EstimatedUrgentDays { get; set; }

    [Required]
    public int EstimatedUrgentHours { get; set; }

    [Required]
    public DateTime UrgentEndTime { get; set; }

    // Kolom baru untuk mencatat waktu crane kembali available secara manual
    public DateTime? ActualUrgentEndTime { get; set; }

    // Kolom untuk menyimpan Hangfire JobId
    public string? HangfireJobId { get; set; }

    [Required]
    public required string Reasons { get; set; }

    public virtual Crane? Crane { get; set; }
  }
}
