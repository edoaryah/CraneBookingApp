// Models/MaintenanceSchedule.cs
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public class MaintenanceSchedule
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public int CraneId { get; set; }

    [Required]
    public required string Title { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    public required string CreatedBy { get; set; }

    public virtual Crane? Crane { get; set; }

    public virtual ICollection<MaintenanceScheduleShift> MaintenanceScheduleShifts { get; set; } = new List<MaintenanceScheduleShift>();
  }

  public class MaintenanceScheduleShift
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public int MaintenanceScheduleId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public int ShiftDefinitionId { get; set; }

    // Properti tambahan untuk menyimpan data historis
    [StringLength(50)]
    public string? ShiftName { get; set; }

    public TimeSpan ShiftStartTime { get; set; }

    public TimeSpan ShiftEndTime { get; set; }

    public virtual MaintenanceSchedule? MaintenanceSchedule { get; set; }

    public virtual ShiftDefinition? ShiftDefinition { get; set; }
  }
}
