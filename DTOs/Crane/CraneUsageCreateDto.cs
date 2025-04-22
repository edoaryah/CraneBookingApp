using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
  public class CraneUsageCreateDto
  {
    public int BookingId { get; set; }
    public DateTime Date { get; set; }
    public UsageCategory Category { get; set; }
    public int SubcategoryId { get; set; }
    public string Duration { get; set; } = "00:00"; // Format: "HH:mm"
  }
}
