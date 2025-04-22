using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
  public class UsageSubcategoryDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public UsageCategory Category { get; set; }
  }
}
