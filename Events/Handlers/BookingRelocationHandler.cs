using System;
using System.Threading.Tasks;
using AspnetCoreMvcFull.Services;
using Microsoft.Extensions.Logging;

namespace AspnetCoreMvcFull.Events.Handlers
{
  public class BookingRelocationHandler : IEventHandler<CraneMaintenanceEvent>
  {
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingRelocationHandler> _logger;

    public BookingRelocationHandler(IBookingService bookingService, ILogger<BookingRelocationHandler> logger)
    {
      _bookingService = bookingService;
      _logger = logger;
    }

    public async Task HandleAsync(CraneMaintenanceEvent @event)
    {
      try
      {
        _logger.LogInformation("Processing booking relocation for crane {CraneId} maintenance", @event.CraneId);

        await _bookingService.RelocateAffectedBookingsAsync(
            @event.CraneId,
            @event.MaintenanceStartTime,
            @event.MaintenanceEndTime);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error relocating bookings for crane {CraneId}", @event.CraneId);
        throw;
      }
    }
  }
}
