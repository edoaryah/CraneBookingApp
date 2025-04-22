using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
  public class CraneDto
  {
    public int Id { get; set; }
    public required string Code { get; set; }
    public int Capacity { get; set; }
    public CraneStatus Status { get; set; }
    public string? ImagePath { get; set; }
  }
}
