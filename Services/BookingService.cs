using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Helpers;

namespace AspnetCoreMvcFull.Services
{
  public class BookingService : IBookingService
  {
    private readonly AppDbContext _context;
    private readonly ICraneService _craneService;
    private readonly IHazardService _hazardService;
    private readonly ILogger<BookingService> _logger;

    public BookingService(AppDbContext context, ICraneService craneService, IHazardService hazardService, ILogger<BookingService> logger)
    {
      _context = context;
      _craneService = craneService;
      _hazardService = hazardService;
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
        StartDate = TimeZoneHelper.UtcToWita(r.StartDate),
        EndDate = TimeZoneHelper.UtcToWita(r.EndDate),
        SubmitTime = TimeZoneHelper.UtcToWita(r.SubmitTime),
        // Tambahkan field baru
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
        StartDate = TimeZoneHelper.UtcToWita(booking.StartDate),
        EndDate = TimeZoneHelper.UtcToWita(booking.EndDate),
        SubmitTime = TimeZoneHelper.UtcToWita(booking.SubmitTime),
        Location = booking.Location,
        ProjectSupervisor = booking.ProjectSupervisor,
        CostCode = booking.CostCode,
        PhoneNumber = booking.PhoneNumber,
        Description = booking.Description,
        Shifts = booking.BookingShifts.Select(s => new BookingShiftDto
        {
          Id = s.Id,
          Date = TimeZoneHelper.UtcToWita(s.Date),
          IsDayShift = s.IsDayShift,
          IsNightShift = s.IsNightShift
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
        StartDate = TimeZoneHelper.UtcToWita(r.StartDate),
        EndDate = TimeZoneHelper.UtcToWita(r.EndDate),
        SubmitTime = TimeZoneHelper.UtcToWita(r.SubmitTime),
        Location = r.Location,
        ProjectSupervisor = r.ProjectSupervisor,
        CostCode = r.CostCode,
        PhoneNumber = r.PhoneNumber,
        Description = r.Description
      }).ToList();
    }

    public async Task<CalendarResponseDto> GetCalendarViewAsync(DateTime startDate, DateTime endDate)
    {
      // Konversi input dates dari WITA ke UTC
      var startDateUtc = TimeZoneHelper.WitaToUtc(startDate.Date);
      var endDateUtc = TimeZoneHelper.WitaToUtc(endDate.Date);

      // Ambil semua crane
      var cranes = await _context.Cranes.ToListAsync();

      // Siapkan response
      var response = new CalendarResponseDto
      {
        WeekRange = new WeekRangeDto
        {
          StartDate = TimeZoneHelper.UtcToWita(startDateUtc).ToString("yyyy-MM-dd"),
          EndDate = TimeZoneHelper.UtcToWita(endDateUtc).ToString("yyyy-MM-dd")
        },
        Cranes = new List<CraneBookingsDto>()
      };

      // Dapatkan semua booking dalam rentang tanggal
      var bookingShifts = await _context.BookingShifts
          .Include(bs => bs.Booking)
          .ThenInclude(b => b!.Crane)
          .Where(bs => bs.Date >= startDateUtc && bs.Date <= endDateUtc)
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

        // Cari booking untuk crane ini
        var craneshifts = bookingShifts
            .Where(bs => bs.Booking!.CraneId == crane.Id)
            .ToList();

        // Tambahkan booking ke list
        foreach (var shift in craneshifts)
        {
          craneDto.Bookings.Add(new BookingCalendarDto
          {
            Id = shift.BookingId,
            BookingNumber = shift.Booking!.BookingNumber,
            Department = shift.Booking.Department,
            Date = TimeZoneHelper.UtcToWita(shift.Date),
            IsDayShift = shift.IsDayShift,
            IsNightShift = shift.IsNightShift
          });
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

        // Konversi input dates dari WITA ke UTC
        var startDateUtc = TimeZoneHelper.WitaToUtc(bookingDto.StartDate.Date);
        var endDateUtc = TimeZoneHelper.WitaToUtc(bookingDto.EndDate.Date);

        // Validate date range
        if (startDateUtc > endDateUtc)
        {
          throw new ArgumentException("Start date must be before or equal to end date");
        }

        // Validate shift selections
        if (bookingDto.ShiftSelections == null || !bookingDto.ShiftSelections.Any())
        {
          throw new ArgumentException("At least one shift selection is required");
        }

        // Check if all dates in the range have shift selections
        var dateRange = Enumerable.Range(0, (endDateUtc - startDateUtc).Days + 1)
            .Select(d => startDateUtc.AddDays(d))
            .ToList();

        var selectedDates = bookingDto.ShiftSelections
            .Select(s => TimeZoneHelper.WitaToUtc(s.Date.Date))
            .ToList();

        if (!dateRange.All(d => selectedDates.Contains(d)))
        {
          throw new ArgumentException("All dates in the range must have shift selections");
        }

        // Validate each shift selection has at least one shift selected
        foreach (var selection in bookingDto.ShiftSelections)
        {
          if (!selection.IsDayShift && !selection.IsNightShift)
          {
            throw new ArgumentException($"At least one shift must be selected for date {TimeZoneHelper.UtcToWita(TimeZoneHelper.WitaToUtc(selection.Date)).ToShortDateString()}");
          }

          // Konversi ke UTC untuk pengecekan konflik
          var dateUtc = TimeZoneHelper.WitaToUtc(selection.Date.Date);

          // Check for scheduling conflicts
          bool hasConflict = await IsBookingConflictAsync(
              bookingDto.CraneId,
              dateUtc,
              selection.IsDayShift,
              selection.IsNightShift);

          if (hasConflict)
          {
            throw new InvalidOperationException($"Scheduling conflict detected for date {TimeZoneHelper.UtcToWita(dateUtc).ToShortDateString()}");
          }
        }

        // Create booking with a temporary booking number (will be updated after we get the ID)
        var booking = new Booking
        {
          BookingNumber = "TEMP", // Temporary value
          Name = bookingDto.Name,
          Department = bookingDto.Department,
          CraneId = bookingDto.CraneId,
          StartDate = startDateUtc,
          EndDate = endDateUtc,
          SubmitTime = DateTime.UtcNow,
          // Tambahkan field baru
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

        // Create shift selections
        foreach (var selection in bookingDto.ShiftSelections)
        {
          var dateUtc = TimeZoneHelper.WitaToUtc(selection.Date.Date);
          var shift = new BookingShift
          {
            BookingId = booking.Id,
            Date = dateUtc,
            IsDayShift = selection.IsDayShift,
            IsNightShift = selection.IsNightShift
          };

          _context.BookingShifts.Add(shift);
        }

        // Add this section to create booking items
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
      var booking = await _context.Bookings
          .Include(r => r.BookingShifts)
          .Include(r => r.BookingItems) // Add this line to include items
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

      // Konversi dates dari WITA ke UTC
      var startDateUtc = TimeZoneHelper.WitaToUtc(bookingDto.StartDate.Date);
      var endDateUtc = TimeZoneHelper.WitaToUtc(bookingDto.EndDate.Date);

      // Validate date range
      if (startDateUtc > endDateUtc)
      {
        throw new ArgumentException("Start date must be before or equal to end date");
      }

      // Validate shift selections
      if (bookingDto.ShiftSelections == null || !bookingDto.ShiftSelections.Any())
      {
        throw new ArgumentException("At least one shift selection is required");
      }

      // Check if all dates in the range have shift selections
      var dateRange = Enumerable.Range(0, (endDateUtc - startDateUtc).Days + 1)
          .Select(d => startDateUtc.AddDays(d))
          .ToList();

      var selectedDates = bookingDto.ShiftSelections
          .Select(s => TimeZoneHelper.WitaToUtc(s.Date.Date))
          .ToList();

      if (!dateRange.All(d => selectedDates.Contains(d)))
      {
        throw new ArgumentException("All dates in the range must have shift selections");
      }

      // Validate each shift selection has at least one shift selected
      foreach (var selection in bookingDto.ShiftSelections)
      {
        if (!selection.IsDayShift && !selection.IsNightShift)
        {
          throw new ArgumentException($"At least one shift must be selected for date {TimeZoneHelper.UtcToWita(TimeZoneHelper.WitaToUtc(selection.Date)).ToShortDateString()}");
        }

        // Konversi ke UTC untuk pengecekan konflik
        var dateUtc = TimeZoneHelper.WitaToUtc(selection.Date.Date);

        // Check for scheduling conflicts (excluding current booking)
        bool hasConflict = await IsBookingConflictAsync(
            bookingDto.CraneId,
            dateUtc,
            selection.IsDayShift,
            selection.IsNightShift,
            id);

        if (hasConflict)
        {
          throw new InvalidOperationException($"Scheduling conflict detected for date {TimeZoneHelper.UtcToWita(dateUtc).ToShortDateString()}");
        }
      }

      // Update booking
      booking.Name = bookingDto.Name;
      booking.Department = bookingDto.Department;
      booking.CraneId = bookingDto.CraneId;
      booking.StartDate = startDateUtc;
      booking.EndDate = endDateUtc;
      booking.CustomHazard = bookingDto.CustomHazard;
      // Tambahkan field baru
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

      // Create new shift selections
      foreach (var selection in bookingDto.ShiftSelections)
      {
        var dateUtc = TimeZoneHelper.WitaToUtc(selection.Date.Date);
        var shift = new BookingShift
        {
          BookingId = booking.Id,
          Date = dateUtc,
          IsDayShift = selection.IsDayShift,
          IsNightShift = selection.IsNightShift
        };

        _context.BookingShifts.Add(shift);
      }

      // Add this section to handle booking items
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

    public async Task DeleteBookingAsync(int id)
    {
      var booking = await _context.Bookings
          .Include(r => r.BookingShifts)
          .Include(r => r.BookingItems) // Add this line to include items
          .FirstOrDefaultAsync(r => r.Id == id);

      if (booking == null)
      {
        throw new KeyNotFoundException($"Booking with ID {id} not found");
      }

      // Remove all associated shifts
      _context.BookingShifts.RemoveRange(booking.BookingShifts);

      // Remove all associated items
      _context.BookingItems.RemoveRange(booking.BookingItems);

      // Remove the booking
      _context.Bookings.Remove(booking);

      await _context.SaveChangesAsync();
    }

    public async Task<bool> IsBookingConflictAsync(int craneId, DateTime date, bool isDayShift, bool isNightShift, int? excludeBookingId = null)
    {
      // Pastikan datetime dalam UTC
      var dateUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

      var query = _context.BookingShifts
          .Include(rs => rs.Booking)
          .Where(rs => rs.Booking!.CraneId == craneId &&
                  rs.Date.Date == dateUtc.Date);

      if (excludeBookingId.HasValue)
      {
        query = query.Where(rs => rs.BookingId != excludeBookingId.Value);
      }

      var existingShifts = await query.ToListAsync();

      // Check for day shift conflict
      if (isDayShift && existingShifts.Any(s => s.IsDayShift))
      {
        return true;
      }

      // Check for night shift conflict
      if (isNightShift && existingShifts.Any(s => s.IsNightShift))
      {
        return true;
      }

      return false;
    }

    public async Task<bool> BookingExistsAsync(int id)
    {
      return await _context.Bookings.AnyAsync(r => r.Id == id);
    }
  }
}
