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
    public DateTime UrgentStartTime { get; set; } = DateTime.UtcNow;

    [Required]
    public int EstimatedUrgentDays { get; set; }

    [Required]
    public int EstimatedUrgentHours { get; set; }

    [Required]
    public DateTime UrgentEndTime { get; set; }

    [Required]
    public required string Reasons { get; set; }

    public virtual Crane? Crane { get; set; }
  }
}
