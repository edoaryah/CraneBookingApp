using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
  public class CraneUsageRecordDto
  {
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string? BookingNumber { get; set; }
    public DateTime Date { get; set; }
    public UsageCategory Category { get; set; }
    public string CategoryName => Category.ToString();
    public int SubcategoryId { get; set; }
    public string SubcategoryName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string DurationFormatted => $"{Duration.Hours:D2}:{Duration.Minutes:D2}";
  }
}
