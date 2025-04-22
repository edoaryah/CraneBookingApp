using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  public class CraneUsageRecord
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public int BookingId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public UsageCategory Category { get; set; }

    [Required]
    public int SubcategoryId { get; set; }

    [Required]
    public TimeSpan Duration { get; set; }

    [ForeignKey("BookingId")]
    public virtual Booking? Booking { get; set; }
  }
}
