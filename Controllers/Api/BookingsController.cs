using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize]
  [ServiceFilter(typeof(GlobalExceptionFilter))]
  public class BookingsController : ControllerBase
  {
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
      _bookingService = bookingService;
    }

    // GET: api/Bookings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings()
    {
      var bookings = await _bookingService.GetAllBookingsAsync();
      return Ok(bookings);
    }

    // GET: api/Bookings/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BookingDetailDto>> GetBooking(int id)
    {
      var booking = await _bookingService.GetBookingByIdAsync(id);
      return Ok(booking);
    }

    // GET: api/Bookings/Crane/5
    [HttpGet("Crane/{craneId}")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookingsByCrane(int craneId)
    {
      var bookings = await _bookingService.GetBookingsByCraneIdAsync(craneId);
      return Ok(bookings);
    }

    [HttpGet("CalendarView")]
    public async Task<ActionResult<CalendarResponseDto>> GetCalendarView(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
      // Jika startDate tidak disediakan, gunakan hari ini
      DateTime start = startDate?.Date ?? DateTime.Today;

      // Jika endDate tidak disediakan, gunakan 6 hari setelah startDate (total 7 hari)
      DateTime end = endDate?.Date ?? start.AddDays(6);

      var calendarData = await _bookingService.GetCalendarViewAsync(start, end);
      return Ok(calendarData);
    }

    // POST: api/Bookings
    [HttpPost]
    public async Task<ActionResult<BookingDetailDto>> CreateBooking(BookingCreateDto bookingDto)
    {
      var result = await _bookingService.CreateBookingAsync(bookingDto);
      return CreatedAtAction(nameof(GetBooking), new { id = result.Id }, result);
    }

    // PUT: api/Bookings/5
    [HttpPut("{id}")]
    public async Task<ActionResult<BookingDetailDto>> UpdateBooking(int id, BookingUpdateDto bookingDto)
    {
      var result = await _bookingService.UpdateBookingAsync(id, bookingDto);
      return Ok(result);
    }

    // DELETE: api/Bookings/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
      await _bookingService.DeleteBookingAsync(id);
      return NoContent();
    }

    // GET: api/Bookings/CheckConflict?craneId=1&date=2025-04-01&isDayShift=true&isNightShift=false
    [HttpGet("CheckConflict")]
    public async Task<ActionResult<bool>> CheckConflict(
        [FromQuery] int craneId,
        [FromQuery] DateTime date,
        [FromQuery] bool isDayShift,
        [FromQuery] bool isNightShift,
        [FromQuery] int? excludeBookingId = null)
    {
      var hasConflict = await _bookingService.IsBookingConflictAsync(
          craneId, date, isDayShift, isNightShift, excludeBookingId);

      return Ok(hasConflict);
    }
  }
}
