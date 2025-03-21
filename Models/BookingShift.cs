using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public class BookingShift
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public int BookingId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public bool IsDayShift { get; set; }

    [Required]
    public bool IsNightShift { get; set; }

    public virtual Booking? Booking { get; set; }
  }
}
