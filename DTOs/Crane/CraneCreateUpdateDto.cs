using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
  public class CraneCreateUpdateDto
  {
    public required string Code { get; set; }
    public int Capacity { get; set; }
    public CraneStatus? Status { get; set; }
    public IFormFile? Image { get; set; }
  }
}
