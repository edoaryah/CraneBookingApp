// DTOs/ShiftDefinitionDto.cs
namespace AspnetCoreMvcFull.DTOs
{
  public class ShiftDefinitionDto
  {
    public int Id { get; set; }
    public required string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
  }

  public class ShiftDefinitionCreateDto
  {
    public required string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
  }

  public class ShiftDefinitionUpdateDto
  {
    public required string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
  }
}
