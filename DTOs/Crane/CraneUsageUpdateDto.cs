using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
  public class CraneUsageUpdateDto
  {
    public UsageCategory Category { get; set; }
    public int SubcategoryId { get; set; }
    public string Duration { get; set; } = "00:00"; // Format: "HH:mm"
  }
}
