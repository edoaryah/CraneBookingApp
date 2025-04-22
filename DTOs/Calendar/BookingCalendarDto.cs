namespace AspnetCoreMvcFull.DTOs
{
  public class BookingCalendarDto
  {
    public int Id { get; set; }
    public required string BookingNumber { get; set; }
    public required string Department { get; set; }
    public DateTime Date { get; set; }
    public List<ShiftBookingDto> Shifts { get; set; } = new List<ShiftBookingDto>();
  }
}
