using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public class BookingService : IBookingService
  {
    private readonly AppDbContext _context;
    private readonly ICraneService _craneService;
    private readonly IHazardService _hazardService;
    private readonly IShiftDefinitionService _shiftDefinitionService;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        AppDbContext context,
        ICraneService craneService,
        IHazardService hazardService,
        IShiftDefinitionService shiftDefinitionService,
        ILogger<BookingService> logger)
    {
      _context = context;
      _craneService = craneService;
      _hazardService = hazardService;
      _shiftDefinitionService = shiftDefinitionService;
      _logger = logger;
    }

    public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
    {
      var bookings = await _context.Bookings
          .Include(r => r.Crane)
          .OrderByDescending(r => r.SubmitTime)
          .ToListAsync();

      return bookings.Select(r => new BookingDto
      {
        Id = r.Id,
        BookingNumber = r.BookingNumber,
        Name = r.Name,
        Department = r.Department,
        CraneId = r.CraneId,
        CraneCode = r.Crane?.Code,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        SubmitTime = r.SubmitTime,
        Location = r.Location,
        ProjectSupervisor = r.ProjectSupervisor,
        CostCode = r.CostCode,
        PhoneNumber = r.PhoneNumber,
        Description = r.Description
      }).ToList();
    }

    public async Task<BookingDetailDto> GetBookingByIdAsync(int id)
    {
      var booking = await _context.Bookings
          .Include(r => r.Crane)
          .Include(r => r.BookingShifts)
            .ThenInclude(bs => bs.ShiftDefinition)
          .Include(r => r.BookingItems)
          .Include(r => r.BookingHazards)
            .ThenInclude(bh => bh.Hazard)
          .FirstOrDefaultAsync(r => r.Id == id);

      if (booking == null)
      {
        throw new KeyNotFoundException($"Booking with ID {id} not found");
      }

      return new BookingDetailDto
      {
        Id = booking.Id,
        BookingNumber = booking.BookingNumber,
        Name = booking.Name,
        Department = booking.Department,
        CraneId = booking.CraneId,
        CraneCode = booking.Crane?.Code,
        StartDate = booking.StartDate,
        EndDate = booking.EndDate,
        SubmitTime = booking.SubmitTime,
        Location = booking.Location,
        ProjectSupervisor = booking.ProjectSupervisor,
        CostCode = booking.CostCode,
        PhoneNumber = booking.PhoneNumber,
        Description = booking.Description,
        Shifts = booking.BookingShifts.Select(s => new BookingShiftDto
        {
          Id = s.Id,
          Date = s.Date,
          ShiftDefinitionId = s.ShiftDefinitionId,
          ShiftName = s.ShiftName ?? s.ShiftDefinition?.Name,
          StartTime = s.ShiftStartTime != default ? s.ShiftStartTime : s.ShiftDefinition?.StartTime,
          EndTime = s.ShiftEndTime != default ? s.ShiftEndTime : s.ShiftDefinition?.EndTime
        }).ToList(),
        Items = booking.BookingItems.Select(i => new BookingItemDto
        {
          Id = i.Id,
          ItemName = i.ItemName,
          Weight = i.Weight,
          Height = i.Height,
          Quantity = i.Quantity
        }).ToList(),
        SelectedHazards = booking.BookingHazards
        .Where(bh => bh.Hazard != null)
        .Select(bh => new HazardDto
        {
          Id = bh.Hazard!.Id,
          Name = bh.Hazard.Name
        }).ToList(),
        CustomHazard = booking.CustomHazard
      };
    }

    public async Task<IEnumerable<BookingDto>> GetBookingsByCraneIdAsync(int craneId)
    {
      if (!await _craneService.CraneExistsAsync(craneId))
      {
        throw new KeyNotFoundException($"Crane with ID {craneId} not found");
      }

      var bookings = await _context.Bookings
          .Include(r => r.Crane)
          .Where(r => r.CraneId == craneId)
          .OrderByDescending(r => r.SubmitTime)
          .ToListAsync();

      return bookings.Select(r => new BookingDto
      {
        Id = r.Id,
        BookingNumber = r.BookingNumber,
        Name = r.Name,
        Department = r.Department,
        CraneId = r.CraneId,
        CraneCode = r.Crane?.Code,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        SubmitTime = r.SubmitTime,
        Location = r.Location,
        ProjectSupervisor = r.ProjectSupervisor,
        CostCode = r.CostCode,
        PhoneNumber = r.PhoneNumber,
        Description = r.Description
      }).ToList();
    }

    public async Task<CalendarResponseDto> GetCalendarViewAsync(DateTime startDate, DateTime endDate)
    {
      // Gunakan langsung date tanpa konversi ke UTC
      var startDateLocal = startDate.Date;
      var endDateLocal = endDate.Date;

      // Ambil semua crane
      var cranes = await _context.Cranes.ToListAsync();

      // Siapkan response
      var response = new CalendarResponseDto
      {
        WeekRange = new WeekRangeDto
        {
          StartDate = startDateLocal.ToString("yyyy-MM-dd"),
          EndDate = endDateLocal.ToString("yyyy-MM-dd")
        },
        Cranes = new List<CraneBookingsDto>()
      };

      // Dapatkan semua booking dalam rentang tanggal
      var bookingShifts = await _context.BookingShifts
          .Include(bs => bs.Booking)
          .ThenInclude(b => b!.Crane)
          .Include(bs => bs.ShiftDefinition)
          .Where(bs => bs.Date >= startDateLocal && bs.Date <= endDateLocal)
          .ToListAsync();

      // Kelompokkan berdasarkan crane
      foreach (var crane in cranes)
      {
        var craneDto = new CraneBookingsDto
        {
          CraneId = crane.Code,
          Capacity = crane.Capacity,
          Bookings = new List<BookingCalendarDto>()
        };

        // Group shifts by date and booking
        var craneShifts = bookingShifts
            .Where(bs => bs.Booking!.CraneId == crane.Id)
            .GroupBy(bs => new { bs.Date, bs.BookingId })
            .ToList();

        foreach (var group in craneShifts)
        {
          // Get first shift to access booking info
          var firstShift = group.First();

          var calendarBooking = new BookingCalendarDto
          {
            Id = firstShift.BookingId,
            BookingNumber = firstShift.Booking!.BookingNumber,
            Department = firstShift.Booking.Department,
            Date = group.Key.Date,
            Shifts = group.Select(s => new ShiftBookingDto
            {
              ShiftDefinitionId = s.ShiftDefinitionId,
              ShiftName = s.ShiftName ?? s.ShiftDefinition?.Name,
              StartTime = s.ShiftStartTime != default ? s.ShiftStartTime : s.ShiftDefinition?.StartTime ?? TimeSpan.Zero,
              EndTime = s.ShiftEndTime != default ? s.ShiftEndTime : s.ShiftDefinition?.EndTime ?? TimeSpan.Zero
            }).ToList()
          };

          craneDto.Bookings.Add(calendarBooking);
        }

        response.Cranes.Add(craneDto);
      }

      return response;
    }

    public async Task<BookingDetailDto> CreateBookingAsync(BookingCreateDto bookingDto)
    {
      try
      {
        _logger.LogInformation("Creating booking for crane {CraneId}", bookingDto.CraneId);

        // Validate crane exists
        if (!await _craneService.CraneExistsAsync(bookingDto.CraneId))
        {
          throw new KeyNotFoundException($"Crane with ID {bookingDto.CraneId} not found");
        }

        // Validate crane is available (not in maintenance)
        var crane = await _context.Cranes.FindAsync(bookingDto.CraneId);
        if (crane?.Status == CraneStatus.Maintenance)
        {
          throw new InvalidOperationException($"Cannot reserve crane with ID {bookingDto.CraneId} because it is currently under maintenance");
        }

        // Gunakan tanggal lokal tanpa konversi UTC
        var startDate = bookingDto.StartDate.Date;
        var endDate = bookingDto.EndDate.Date;

        // Validate date range
        if (startDate > endDate)
        {
          throw new ArgumentException("Start date must be before or equal to end date");
        }

        // Validate shift selections
        if (bookingDto.ShiftSelections == null || !bookingDto.ShiftSelections.Any())
        {
          throw new ArgumentException("At least one shift selection is required");
        }

        // Check if all dates in the range have shift selections
        var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
            .Select(d => startDate.AddDays(d))
            .ToList();

        var selectedDates = bookingDto.ShiftSelections
            .Select(s => s.Date.Date)
            .ToList();

        if (!dateRange.All(d => selectedDates.Contains(d)))
        {
          throw new ArgumentException("All dates in the range must have shift selections");
        }

        // Validate each shift selection has at least one shift selected
        foreach (var selection in bookingDto.ShiftSelections)
        {
          if (selection.SelectedShiftIds == null || !selection.SelectedShiftIds.Any())
          {
            throw new ArgumentException($"At least one shift must be selected for date {selection.Date.ToShortDateString()}");
          }

          // Gunakan tanggal lokal untuk pengecekan konflik
          var dateLocal = selection.Date.Date;

          // Check for scheduling conflicts for each selected shift
          foreach (var shiftId in selection.SelectedShiftIds)
          {
            // Verify the shift definition exists
            if (!await _shiftDefinitionService.ShiftDefinitionExistsAsync(shiftId))
            {
              throw new KeyNotFoundException($"Shift definition with ID {shiftId} not found");
            }

            bool hasConflict = await IsShiftBookingConflictAsync(
                bookingDto.CraneId,
                dateLocal,
                shiftId);

            if (hasConflict)
            {
              // Get shift name for better error message
              var shift = await _context.ShiftDefinitions.FindAsync(shiftId);
              throw new InvalidOperationException($"Scheduling conflict detected for date {dateLocal.ToShortDateString()} and shift {shift?.Name ?? shiftId.ToString()}");
            }
          }
        }

        // Create booking with a temporary booking number (will be updated after we get the ID)
        var booking = new Booking
        {
          BookingNumber = "TEMP", // Temporary value
          Name = bookingDto.Name,
          Department = bookingDto.Department,
          CraneId = bookingDto.CraneId,
          StartDate = startDate,
          EndDate = endDate,
          SubmitTime = DateTime.Now,
          Location = bookingDto.Location,
          ProjectSupervisor = bookingDto.ProjectSupervisor,
          CostCode = bookingDto.CostCode,
          PhoneNumber = bookingDto.PhoneNumber,
          Description = bookingDto.Description,
          CustomHazard = bookingDto.CustomHazard
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Update booking number based on ID
        booking.BookingNumber = $"C{booking.Id:D4}";
        await _context.SaveChangesAsync();

        // Create shift selections with historical data
        foreach (var selection in bookingDto.ShiftSelections)
        {
          var dateLocal = selection.Date.Date;

          foreach (var shiftId in selection.SelectedShiftIds)
          {
            // Dapatkan informasi shift saat ini
            var shiftDefinition = await _context.ShiftDefinitions.FindAsync(shiftId);
            if (shiftDefinition == null)
            {
              throw new KeyNotFoundException($"Shift definition with ID {shiftId} not found");
            }

            var bookingShift = new BookingShift
            {
              BookingId = booking.Id,
              Date = dateLocal,
              ShiftDefinitionId = shiftId,
              // Simpan juga data historis shift
              ShiftName = shiftDefinition.Name,
              ShiftStartTime = shiftDefinition.StartTime,
              ShiftEndTime = shiftDefinition.EndTime
            };

            _context.BookingShifts.Add(bookingShift);
          }
        }

        // Add booking items
        if (bookingDto.Items != null && bookingDto.Items.Any())
        {
          foreach (var itemDto in bookingDto.Items)
          {
            var item = new BookingItem
            {
              BookingId = booking.Id,
              ItemName = itemDto.ItemName,
              Weight = itemDto.Weight,
              Height = itemDto.Height,
              Quantity = itemDto.Quantity
            };

            _context.BookingItems.Add(item);
          }
        }

        // Handle predefined hazards
        if (bookingDto.HazardIds != null && bookingDto.HazardIds.Any())
        {
          foreach (var hazardId in bookingDto.HazardIds)
          {
            // Validasi hazard exists
            if (await _hazardService.HazardExistsAsync(hazardId))
            {
              var bookingHazard = new BookingHazard
              {
                BookingId = booking.Id,
                HazardId = hazardId
              };
              _context.BookingHazards.Add(bookingHazard);
            }
          }
        }

        await _context.SaveChangesAsync();

        // Return the created booking with details
        return await GetBookingByIdAsync(booking.Id);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating booking: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<BookingDetailDto> UpdateBookingAsync(int id, BookingUpdateDto bookingDto)
    {
      try
      {
        _logger.LogInformation("Updating booking ID: {Id}", id);

        var booking = await _context.Bookings
            .Include(r => r.BookingShifts)
            .Include(r => r.BookingItems)
            .Include(r => r.BookingHazards)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (booking == null)
        {
          throw new KeyNotFoundException($"Booking with ID {id} not found");
        }

        // Validate crane exists if changing crane
        if (booking.CraneId != bookingDto.CraneId &&
            !await _craneService.CraneExistsAsync(bookingDto.CraneId))
        {
          throw new KeyNotFoundException($"Crane with ID {bookingDto.CraneId} not found");
        }

        // Validate crane is available if changing crane
        if (booking.CraneId != bookingDto.CraneId)
        {
          var crane = await _context.Cranes.FindAsync(bookingDto.CraneId);
          if (crane?.Status == CraneStatus.Maintenance)
          {
            throw new InvalidOperationException($"Cannot reserve crane with ID {bookingDto.CraneId} because it is currently under maintenance");
          }
        }

        // Gunakan tanggal lokal tanpa konversi UTC
        var startDate = bookingDto.StartDate.Date;
        var endDate = bookingDto.EndDate.Date;

        // Validate date range
        if (startDate > endDate)
        {
          throw new ArgumentException("Start date must be before or equal to end date");
        }

        // Validate shift selections
        if (bookingDto.ShiftSelections == null || !bookingDto.ShiftSelections.Any())
        {
          throw new ArgumentException("At least one shift selection is required");
        }

        // Check if all dates in the range have shift selections
        var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
            .Select(d => startDate.AddDays(d))
            .ToList();

        var selectedDates = bookingDto.ShiftSelections
            .Select(s => s.Date.Date)
            .ToList();

        if (!dateRange.All(d => selectedDates.Contains(d)))
        {
          throw new ArgumentException("All dates in the range must have shift selections");
        }

        // Validate each shift selection has at least one shift selected
        foreach (var selection in bookingDto.ShiftSelections)
        {
          if (selection.SelectedShiftIds == null || !selection.SelectedShiftIds.Any())
          {
            throw new ArgumentException($"At least one shift must be selected for date {selection.Date.ToShortDateString()}");
          }

          // Gunakan tanggal lokal untuk pengecekan konflik
          var dateLocal = selection.Date.Date;

          // Check for scheduling conflicts for each selected shift
          foreach (var shiftId in selection.SelectedShiftIds)
          {
            // Verify the shift definition exists
            if (!await _shiftDefinitionService.ShiftDefinitionExistsAsync(shiftId))
            {
              throw new KeyNotFoundException($"Shift definition with ID {shiftId} not found");
            }

            bool hasConflict = await IsShiftBookingConflictAsync(
                bookingDto.CraneId,
                dateLocal,
                shiftId,
                id);

            if (hasConflict)
            {
              // Get shift name for better error message
              var shift = await _context.ShiftDefinitions.FindAsync(shiftId);
              throw new InvalidOperationException($"Scheduling conflict detected for date {dateLocal.ToShortDateString()} and shift {shift?.Name ?? shiftId.ToString()}");
            }
          }
        }

        // Update booking
        booking.Name = bookingDto.Name;
        booking.Department = bookingDto.Department;
        booking.CraneId = bookingDto.CraneId;
        booking.StartDate = startDate;
        booking.EndDate = endDate;
        booking.CustomHazard = bookingDto.CustomHazard;
        booking.Location = bookingDto.Location;
        booking.ProjectSupervisor = bookingDto.ProjectSupervisor;
        booking.CostCode = bookingDto.CostCode;
        booking.PhoneNumber = bookingDto.PhoneNumber;
        booking.Description = bookingDto.Description;
        // SubmitTime is not updated

        // Remove existing shift selections
        _context.BookingShifts.RemoveRange(booking.BookingShifts);

        // Remove existing hazards
        _context.BookingHazards.RemoveRange(booking.BookingHazards);

        // Create new shift selections with historical data
        foreach (var selection in bookingDto.ShiftSelections)
        {
          var dateLocal = selection.Date.Date;

          foreach (var shiftId in selection.SelectedShiftIds)
          {
            // Dapatkan informasi shift saat ini
            var shiftDefinition = await _context.ShiftDefinitions.FindAsync(shiftId);
            if (shiftDefinition == null)
            {
              throw new KeyNotFoundException($"Shift definition with ID {shiftId} not found");
            }

            var bookingShift = new BookingShift
            {
              BookingId = booking.Id,
              Date = dateLocal,
              ShiftDefinitionId = shiftId,
              // Simpan juga data historis shift
              ShiftName = shiftDefinition.Name,
              ShiftStartTime = shiftDefinition.StartTime,
              ShiftEndTime = shiftDefinition.EndTime
            };

            _context.BookingShifts.Add(bookingShift);
          }
        }

        // Remove existing items
        _context.BookingItems.RemoveRange(booking.BookingItems);

        // Add new items
        if (bookingDto.Items != null && bookingDto.Items.Any())
        {
          foreach (var itemDto in bookingDto.Items)
          {
            var item = new BookingItem
            {
              BookingId = booking.Id,
              ItemName = itemDto.ItemName,
              Weight = itemDto.Weight,
              Height = itemDto.Height,
              Quantity = itemDto.Quantity
            };

            _context.BookingItems.Add(item);
          }
        }

        // Handle predefined hazards
        if (bookingDto.HazardIds != null && bookingDto.HazardIds.Any())
        {
          foreach (var hazardId in bookingDto.HazardIds)
          {
            // Validasi hazard exists
            if (await _hazardService.HazardExistsAsync(hazardId))
            {
              var bookingHazard = new BookingHazard
              {
                BookingId = booking.Id,
                HazardId = hazardId
              };
              _context.BookingHazards.Add(bookingHazard);
            }
          }
        }

        await _context.SaveChangesAsync();

        // Return the updated booking with details
        return await GetBookingByIdAsync(booking.Id);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating booking: {Message}", ex.Message);
        throw;
      }
    }

    public async Task DeleteBookingAsync(int id)
    {
      try
      {
        _logger.LogInformation("Deleting booking ID: {Id}", id);

        var booking = await _context.Bookings
            .Include(r => r.BookingShifts)
            .Include(r => r.BookingItems)
            .Include(r => r.BookingHazards)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (booking == null)
        {
          throw new KeyNotFoundException($"Booking with ID {id} not found");
        }

        // Remove all associated shifts
        _context.BookingShifts.RemoveRange(booking.BookingShifts);

        // Remove all associated items
        _context.BookingItems.RemoveRange(booking.BookingItems);

        // Remove all associated hazards
        _context.BookingHazards.RemoveRange(booking.BookingHazards);

        // Remove the booking
        _context.Bookings.Remove(booking);

        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting booking: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<bool> IsShiftBookingConflictAsync(int craneId, DateTime date, int shiftDefinitionId, int? excludeBookingId = null)
    {
      // Gunakan tanggal lokal tanpa konversi
      var dateLocal = date.Date;

      var query = _context.BookingShifts
          .Include(rs => rs.Booking)
          .Where(rs => rs.Booking!.CraneId == craneId &&
                  rs.Date.Date == dateLocal &&
                  rs.ShiftDefinitionId == shiftDefinitionId);

      if (excludeBookingId.HasValue)
      {
        query = query.Where(rs => rs.BookingId != excludeBookingId.Value);
      }

      var existingBookings = await query.AnyAsync();
      return existingBookings;
    }

    public async Task<bool> BookingExistsAsync(int id)
    {
      return await _context.Bookings.AnyAsync(r => r.Id == id);
    }
  }
}
