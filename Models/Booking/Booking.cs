// [Models/Booking.cs]
// Penambahan properti untuk approval (Status, ManagerName, ManagerApprovalTime, dll).
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public enum BookingStatus
  {
    Pending,
    ManagerApproved,
    Approved,
    ManagerRejected,
    Rejected,
    Done
  }

  public class Booking
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public required string BookingNumber { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Department { get; set; }

    [Required]
    public int CraneId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public DateTime SubmitTime { get; set; } = DateTime.Now;

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(100)]
    public string? ProjectSupervisor { get; set; }

    [StringLength(50)]
    public string? CostCode { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? CustomHazard { get; set; }

    // Kolom untuk approval
    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    // Data Manager
    public string? ManagerName { get; set; }
    public DateTime? ManagerApprovalTime { get; set; }
    [StringLength(500)]
    public string? ManagerRejectReason { get; set; }

    // Data PIC Crane
    public string? PicName { get; set; }
    public DateTime? PicApprovalTime { get; set; }
    [StringLength(500)]
    public string? PicRejectReason { get; set; }

    public virtual Crane? Crane { get; set; }

    public virtual ICollection<BookingShift> BookingShifts { get; set; } = new List<BookingShift>();

    public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();

    public virtual ICollection<BookingHazard> BookingHazards { get; set; } = new List<BookingHazard>();
  }
}
