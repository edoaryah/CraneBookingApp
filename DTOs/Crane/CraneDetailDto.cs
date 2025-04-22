using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
  public class CraneDetailDto
  {
    public int Id { get; set; }
    public required string Code { get; set; }
    public int Capacity { get; set; }
    public CraneStatus Status { get; set; }
    public string? ImagePath { get; set; }
    public ICollection<BreakdownDto> Breakdowns { get; set; } = new List<BreakdownDto>();
  }
}
