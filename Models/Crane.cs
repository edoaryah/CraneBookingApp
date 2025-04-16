using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public enum CraneStatus
  {
    Available,
    Maintenance
  }

  public class Crane
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public required string Code { get; set; }

    [Required]
    public int Capacity { get; set; }

    [Required]
    public CraneStatus Status { get; set; } = CraneStatus.Available;

    // Properti baru untuk gambar
    public string? ImagePath { get; set; }

    public ICollection<UrgentLog> UrgentLogs { get; set; } = new List<UrgentLog>();
  }
}
