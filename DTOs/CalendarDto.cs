namespace AspnetCoreMvcFull.DTOs
{
  public class WeekRangeDto
  {
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
  }

  public class CraneBookingsDto
  {
    public required string CraneId { get; set; }
    public int Capacity { get; set; }
    public List<BookingCalendarDto> Bookings { get; set; } = new List<BookingCalendarDto>();
  }

  public class CalendarResponseDto
  {
    public required WeekRangeDto WeekRange { get; set; }
    public List<CraneBookingsDto> Cranes { get; set; } = new List<CraneBookingsDto>();
  }
}
