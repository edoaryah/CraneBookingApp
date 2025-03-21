namespace AspnetCoreMvcFull.DTOs
{
  public class BookingShiftDto
  {
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public bool IsDayShift { get; set; }
    public bool IsNightShift { get; set; }
  }

  public class DailyShiftSelectionDto
  {
    public DateTime Date { get; set; }
    public bool IsDayShift { get; set; }
    public bool IsNightShift { get; set; }
  }

  public class BookingCalendarDto
  {
    public int Id { get; set; }
    public required string BookingNumber { get; set; }
    public required string Department { get; set; }
    public DateTime Date { get; set; }
    public bool IsDayShift { get; set; }
    public bool IsNightShift { get; set; }
  }
}
