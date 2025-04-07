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
    public List<MaintenanceCalendarDto> MaintenanceSchedules { get; set; } = new List<MaintenanceCalendarDto>();
  }

  public class MaintenanceCalendarDto
  {
    public int Id { get; set; }
    public required string Title { get; set; }
    public DateTime Date { get; set; }
    public List<ShiftBookingDto> Shifts { get; set; } = new List<ShiftBookingDto>();
  }

  public class CalendarResponseDto
  {
    public required WeekRangeDto WeekRange { get; set; }
    public List<CraneBookingsDto> Cranes { get; set; } = new List<CraneBookingsDto>();
  }
}
