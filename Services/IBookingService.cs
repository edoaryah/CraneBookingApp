using AspnetCoreMvcFull.DTOs;

namespace AspnetCoreMvcFull.Services
{
  public interface IBookingService
  {
    Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
    Task<BookingDetailDto> GetBookingByIdAsync(int id);
    Task<IEnumerable<BookingDto>> GetBookingsByCraneIdAsync(int craneId);
    Task<CalendarResponseDto> GetCalendarViewAsync(DateTime startDate, DateTime endDate);
    Task<BookingDetailDto> CreateBookingAsync(BookingCreateDto bookingDto);
    Task<BookingDetailDto> UpdateBookingAsync(int id, BookingUpdateDto bookingDto);
    Task DeleteBookingAsync(int id);
    Task<bool> IsBookingConflictAsync(int craneId, DateTime date, bool isDayShift, bool isNightShift, int? excludeBookingId = null);
    Task<bool> BookingExistsAsync(int id);
  }
}
