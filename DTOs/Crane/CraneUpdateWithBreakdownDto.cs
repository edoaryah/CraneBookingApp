namespace AspnetCoreMvcFull.DTOs
{
  public class CraneUpdateWithBreakdownDto
  {
    public required CraneCreateUpdateDto Crane { get; set; }
    public BreakdownCreateDto? Breakdown { get; set; }
  }
}
