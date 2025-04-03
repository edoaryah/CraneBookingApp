namespace AspnetCoreMvcFull.DTOs
{
  public class BookingShiftDto
  {
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int ShiftDefinitionId { get; set; }
    public string? ShiftName { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
  }

  public class DailyShiftSelectionDto
  {
    public DateTime Date { get; set; }
    public List<int> SelectedShiftIds { get; set; } = new List<int>();
  }

  public class BookingCalendarDto
  {
    public int Id { get; set; }
    public required string BookingNumber { get; set; }
    public required string Department { get; set; }
    public DateTime Date { get; set; }
    public List<ShiftBookingDto> Shifts { get; set; } = new List<ShiftBookingDto>();
  }

  public class ShiftBookingDto
  {
    public int ShiftDefinitionId { get; set; }
    public string? ShiftName { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
  }
}
