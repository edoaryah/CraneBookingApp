// // Services/CraneUsageService.cs
// using Microsoft.EntityFrameworkCore;
// using AspnetCoreMvcFull.Data;
// using AspnetCoreMvcFull.DTOs;
// using AspnetCoreMvcFull.Models;

// namespace AspnetCoreMvcFull.Services
// {
//   public class CraneUsageService : ICraneUsageService
//   {
//     private readonly AppDbContext _context;
//     private readonly IBookingService _bookingService;
//     private readonly ILogger<CraneUsageService> _logger;

//     private static readonly Dictionary<UsageCategory, List<UsageSubcategory>> _categorySubcategories = new()
//         {
//             {
//                 UsageCategory.Operating, new List<UsageSubcategory>
//                 {
//                     UsageSubcategory.Pengangkatan,
//                     UsageSubcategory.MenggantungBeban
//                 }
//             },
//             {
//                 UsageCategory.Delay, new List<UsageSubcategory>
//                 {
//                     UsageSubcategory.MenungguUser,
//                     UsageSubcategory.MenungguKesiapanPengangkatan,
//                     UsageSubcategory.MenungguPengawalan
//                 }
//             },
//             {
//                 UsageCategory.Standby, new List<UsageSubcategory>
//                 {
//                     UsageSubcategory.TidakSedangDiperlukan,
//                     UsageSubcategory.TidakAdaOperator,
//                     UsageSubcategory.TidakAdaPengawal,
//                     UsageSubcategory.Istirahat,
//                     UsageSubcategory.GantiShift,
//                     UsageSubcategory.TidakBisaLewat
//                 }
//             },
//             {
//                 UsageCategory.Service, new List<UsageSubcategory>
//                 {
//                     UsageSubcategory.ServisRutinTerjadwal
//                 }
//             },
//             {
//                 UsageCategory.Breakdown, new List<UsageSubcategory>
//                 {
//                     UsageSubcategory.Rusak,
//                     UsageSubcategory.Perbaikan
//                 }
//             }
//         };

//     public CraneUsageService(
//         AppDbContext context,
//         IBookingService bookingService,
//         ILogger<CraneUsageService> logger)
//     {
//       _context = context;
//       _bookingService = bookingService;
//       _logger = logger;
//     }

//     public async Task<IEnumerable<CraneUsageRecordDto>> GetAllUsageRecordsAsync()
//     {
//       var records = await _context.CraneUsageRecords
//           .Include(r => r.Booking)
//           .OrderByDescending(r => r.Date)
//           .ToListAsync();

//       return records.Select(MapToDto).ToList();
//     }

//     public async Task<IEnumerable<CraneUsageRecordDto>> GetUsageRecordsByBookingIdAsync(int bookingId)
//     {
//       if (!await _bookingService.BookingExistsAsync(bookingId))
//       {
//         throw new KeyNotFoundException($"Booking with ID {bookingId} not found");
//       }

//       var records = await _context.CraneUsageRecords
//           .Include(r => r.Booking)
//           .Where(r => r.BookingId == bookingId)
//           .OrderByDescending(r => r.Date)
//           .ToListAsync();

//       return records.Select(MapToDto).ToList();
//     }

//     public async Task<BookingUsageSummaryDto> GetBookingUsageSummaryAsync(int bookingId)
//     {
//       var booking = await _context.Bookings
//           .FirstOrDefaultAsync(b => b.Id == bookingId);

//       if (booking == null)
//       {
//         throw new KeyNotFoundException($"Booking with ID {bookingId} not found");
//       }

//       var records = await _context.CraneUsageRecords
//           .Where(r => r.BookingId == bookingId)
//           .OrderByDescending(r => r.Date)
//           .ToListAsync();

//       var recordDtos = records.Select(MapToDto).ToList();

//       return new BookingUsageSummaryDto
//       {
//         BookingId = booking.Id,
//         BookingNumber = booking.BookingNumber,
//         Date = booking.StartDate, // Using start date as reference
//         UsageRecords = recordDtos
//       };
//     }

//     public async Task<CraneUsageRecordDto> GetUsageRecordByIdAsync(int id)
//     {
//       var record = await _context.CraneUsageRecords
//           .Include(r => r.Booking)
//           .FirstOrDefaultAsync(r => r.Id == id);

//       if (record == null)
//       {
//         throw new KeyNotFoundException($"Usage record with ID {id} not found");
//       }

//       return MapToDto(record);
//     }

//     public async Task<CraneUsageRecordDto> CreateUsageRecordAsync(CraneUsageCreateDto recordDto)
//     {
//       try
//       {
//         _logger.LogInformation("Creating usage record for booking {BookingId}", recordDto.BookingId);

//         // Validate booking exists
//         if (!await _bookingService.BookingExistsAsync(recordDto.BookingId))
//         {
//           throw new KeyNotFoundException($"Booking with ID {recordDto.BookingId} not found");
//         }

//         // Validate subcategory is valid for the selected category
//         if (!IsValidSubcategory(recordDto.Category, recordDto.Subcategory))
//         {
//           throw new ArgumentException($"Subcategory {recordDto.Subcategory} is not valid for category {recordDto.Category}");
//         }

//         // Parse duration string (format: "HH:mm")
//         if (!TryParseTimeSpan(recordDto.Duration, out TimeSpan duration))
//         {
//           throw new ArgumentException("Invalid duration format. Please use the format HH:mm (e.g., 02:15 for 2 hours and 15 minutes)");
//         }

//         var record = new CraneUsageRecord
//         {
//           BookingId = recordDto.BookingId,
//           Date = recordDto.Date.Date, // Store date only
//           Category = recordDto.Category,
//           Subcategory = recordDto.Subcategory,
//           Duration = duration
//         };

//         _context.CraneUsageRecords.Add(record);
//         await _context.SaveChangesAsync();

//         // Load the booking for the response
//         await _context.Entry(record)
//             .Reference(r => r.Booking)
//             .LoadAsync();

//         return MapToDto(record);
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "Error creating usage record: {Message}", ex.Message);
//         throw;
//       }
//     }

//     public async Task<CraneUsageRecordDto> UpdateUsageRecordAsync(int id, CraneUsageUpdateDto recordDto)
//     {
//       try
//       {
//         _logger.LogInformation("Updating usage record ID: {Id}", id);

//         var record = await _context.CraneUsageRecords
//             .Include(r => r.Booking)
//             .FirstOrDefaultAsync(r => r.Id == id);

//         if (record == null)
//         {
//           throw new KeyNotFoundException($"Usage record with ID {id} not found");
//         }

//         // Validate subcategory is valid for the selected category
//         if (!IsValidSubcategory(recordDto.Category, recordDto.Subcategory))
//         {
//           throw new ArgumentException($"Subcategory {recordDto.Subcategory} is not valid for category {recordDto.Category}");
//         }

//         // Parse duration string (format: "HH:mm")
//         if (!TryParseTimeSpan(recordDto.Duration, out TimeSpan duration))
//         {
//           throw new ArgumentException("Invalid duration format. Please use the format HH:mm (e.g., 02:15 for 2 hours and 15 minutes)");
//         }

//         // Update record
//         record.Category = recordDto.Category;
//         record.Subcategory = recordDto.Subcategory;
//         record.Duration = duration;

//         await _context.SaveChangesAsync();

//         return MapToDto(record);
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "Error updating usage record: {Message}", ex.Message);
//         throw;
//       }
//     }

//     public async Task DeleteUsageRecordAsync(int id)
//     {
//       try
//       {
//         _logger.LogInformation("Deleting usage record ID: {Id}", id);

//         var record = await _context.CraneUsageRecords.FindAsync(id);

//         if (record == null)
//         {
//           throw new KeyNotFoundException($"Usage record with ID {id} not found");
//         }

//         _context.CraneUsageRecords.Remove(record);
//         await _context.SaveChangesAsync();
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "Error deleting usage record: {Message}", ex.Message);
//         throw;
//       }
//     }

//     public async Task<bool> UsageRecordExistsAsync(int id)
//     {
//       return await _context.CraneUsageRecords.AnyAsync(r => r.Id == id);
//     }

//     public async Task<IEnumerable<UsageSubcategory>> GetSubcategoriesForCategoryAsync(UsageCategory category)
//     {
//       // This could be a database query if subcategories were stored in the database
//       // For now, we use the static dictionary mapping
//       return _categorySubcategories.TryGetValue(category, out var subcategories)
//           ? subcategories
//           : Enumerable.Empty<UsageSubcategory>();
//     }

//     private static CraneUsageRecordDto MapToDto(CraneUsageRecord record)
//     {
//       return new CraneUsageRecordDto
//       {
//         Id = record.Id,
//         BookingId = record.BookingId,
//         BookingNumber = record.Booking?.BookingNumber,
//         Date = record.Date,
//         Category = record.Category,
//         Subcategory = record.Subcategory,
//         Duration = record.Duration
//       };
//     }

//     private static bool IsValidSubcategory(UsageCategory category, UsageSubcategory subcategory)
//     {
//       return _categorySubcategories.TryGetValue(category, out var subcategories) &&
//              subcategories.Contains(subcategory);
//     }

//     private static bool TryParseTimeSpan(string timeString, out TimeSpan result)
//     {
//       // Parse format: "HH:mm"
//       if (TimeSpan.TryParseExact(timeString, @"hh\:mm", null, out result))
//       {
//         return true;
//       }

//       // Try alternative format: "H:mm"
//       return TimeSpan.TryParseExact(timeString, @"h\:mm", null, out result);
//     }
//   }
// }

using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public class CraneUsageService : ICraneUsageService
  {
    private readonly AppDbContext _context;
    private readonly IBookingService _bookingService;
    private readonly ILogger<CraneUsageService> _logger;
    private readonly IUsageSubcategoryService _subcategoryService;

    public CraneUsageService(
        AppDbContext context,
        IBookingService bookingService,
        IUsageSubcategoryService subcategoryService,
        ILogger<CraneUsageService> logger)
    {
      _context = context;
      _bookingService = bookingService;
      _subcategoryService = subcategoryService;
      _logger = logger;
    }

    public async Task<IEnumerable<CraneUsageRecordDto>> GetAllUsageRecordsAsync()
    {
      var records = await _context.CraneUsageRecords
          .Include(r => r.Booking)
          .OrderByDescending(r => r.Date)
          .ToListAsync();

      var dtos = new List<CraneUsageRecordDto>();

      foreach (var record in records)
      {
        var subcategory = await _context.UsageSubcategories
            .FirstOrDefaultAsync(s => s.Id == record.SubcategoryId);

        var dto = MapToDto(record, subcategory);
        dtos.Add(dto);
      }

      return dtos;
    }

    public async Task<IEnumerable<CraneUsageRecordDto>> GetUsageRecordsByBookingIdAsync(int bookingId)
    {
      if (!await _bookingService.BookingExistsAsync(bookingId))
      {
        throw new KeyNotFoundException($"Booking with ID {bookingId} not found");
      }

      var records = await _context.CraneUsageRecords
          .Include(r => r.Booking)
          .Where(r => r.BookingId == bookingId)
          .OrderByDescending(r => r.Date)
          .ToListAsync();

      var dtos = new List<CraneUsageRecordDto>();

      foreach (var record in records)
      {
        var subcategory = await _context.UsageSubcategories
            .FirstOrDefaultAsync(s => s.Id == record.SubcategoryId);

        var dto = MapToDto(record, subcategory);
        dtos.Add(dto);
      }

      return dtos;
    }

    public async Task<BookingUsageSummaryDto> GetBookingUsageSummaryAsync(int bookingId)
    {
      var booking = await _context.Bookings
          .FirstOrDefaultAsync(b => b.Id == bookingId);

      if (booking == null)
      {
        throw new KeyNotFoundException($"Booking with ID {bookingId} not found");
      }

      var records = await _context.CraneUsageRecords
          .Where(r => r.BookingId == bookingId)
          .OrderByDescending(r => r.Date)
          .ToListAsync();

      var dtos = new List<CraneUsageRecordDto>();

      foreach (var record in records)
      {
        var subcategory = await _context.UsageSubcategories
            .FirstOrDefaultAsync(s => s.Id == record.SubcategoryId);

        var dto = MapToDto(record, subcategory);
        dtos.Add(dto);
      }

      return new BookingUsageSummaryDto
      {
        BookingId = booking.Id,
        BookingNumber = booking.BookingNumber,
        Date = booking.StartDate, // Using start date as reference
        UsageRecords = dtos
      };
    }

    public async Task<CraneUsageRecordDto> GetUsageRecordByIdAsync(int id)
    {
      var record = await _context.CraneUsageRecords
          .Include(r => r.Booking)
          .FirstOrDefaultAsync(r => r.Id == id);

      if (record == null)
      {
        throw new KeyNotFoundException($"Usage record with ID {id} not found");
      }

      var subcategory = await _context.UsageSubcategories
          .FirstOrDefaultAsync(s => s.Id == record.SubcategoryId);

      return MapToDto(record, subcategory);
    }

    public async Task<CraneUsageRecordDto> CreateUsageRecordAsync(CraneUsageCreateDto recordDto)
    {
      try
      {
        _logger.LogInformation("Creating usage record for booking {BookingId}", recordDto.BookingId);

        // Validate booking exists
        if (!await _bookingService.BookingExistsAsync(recordDto.BookingId))
        {
          throw new KeyNotFoundException($"Booking with ID {recordDto.BookingId} not found");
        }

        // Validate subcategory exists and belongs to the selected category
        var subcategory = await _context.UsageSubcategories
            .FirstOrDefaultAsync(s => s.Id == recordDto.SubcategoryId && s.IsActive);

        if (subcategory == null)
        {
          throw new KeyNotFoundException($"Subcategory with ID {recordDto.SubcategoryId} not found");
        }

        if (subcategory.Category != recordDto.Category)
        {
          throw new ArgumentException($"Subcategory {subcategory.Name} does not belong to category {recordDto.Category}");
        }

        // Parse duration string (format: "HH:mm")
        if (!TryParseTimeSpan(recordDto.Duration, out TimeSpan duration))
        {
          throw new ArgumentException("Invalid duration format. Please use the format HH:mm (e.g., 02:15 for 2 hours and 15 minutes)");
        }

        var record = new CraneUsageRecord
        {
          BookingId = recordDto.BookingId,
          Date = recordDto.Date.Date, // Store date only
          Category = recordDto.Category,
          SubcategoryId = recordDto.SubcategoryId,
          Duration = duration
        };

        _context.CraneUsageRecords.Add(record);
        await _context.SaveChangesAsync();

        // Load the booking for the response
        await _context.Entry(record)
            .Reference(r => r.Booking)
            .LoadAsync();

        return MapToDto(record, subcategory);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating usage record: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<CraneUsageRecordDto> UpdateUsageRecordAsync(int id, CraneUsageUpdateDto recordDto)
    {
      try
      {
        _logger.LogInformation("Updating usage record ID: {Id}", id);

        var record = await _context.CraneUsageRecords
            .Include(r => r.Booking)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (record == null)
        {
          throw new KeyNotFoundException($"Usage record with ID {id} not found");
        }

        // Validate subcategory exists and belongs to the selected category
        var subcategory = await _context.UsageSubcategories
            .FirstOrDefaultAsync(s => s.Id == recordDto.SubcategoryId && s.IsActive);

        if (subcategory == null)
        {
          throw new KeyNotFoundException($"Subcategory with ID {recordDto.SubcategoryId} not found");
        }

        if (subcategory.Category != recordDto.Category)
        {
          throw new ArgumentException($"Subcategory {subcategory.Name} does not belong to category {recordDto.Category}");
        }

        // Parse duration string (format: "HH:mm")
        if (!TryParseTimeSpan(recordDto.Duration, out TimeSpan duration))
        {
          throw new ArgumentException("Invalid duration format. Please use the format HH:mm (e.g., 02:15 for 2 hours and 15 minutes)");
        }

        // Update record
        record.Category = recordDto.Category;
        record.SubcategoryId = recordDto.SubcategoryId;
        record.Duration = duration;

        await _context.SaveChangesAsync();

        return MapToDto(record, subcategory);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating usage record: {Message}", ex.Message);
        throw;
      }
    }

    public async Task DeleteUsageRecordAsync(int id)
    {
      try
      {
        _logger.LogInformation("Deleting usage record ID: {Id}", id);

        var record = await _context.CraneUsageRecords.FindAsync(id);

        if (record == null)
        {
          throw new KeyNotFoundException($"Usage record with ID {id} not found");
        }

        _context.CraneUsageRecords.Remove(record);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting usage record: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<bool> UsageRecordExistsAsync(int id)
    {
      return await _context.CraneUsageRecords.AnyAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<UsageSubcategoryDto>> GetSubcategoriesForCategoryAsync(UsageCategory category)
    {
      return await _subcategoryService.GetSubcategoriesByCategoryAsync(category);
    }

    // Updated mapper method to accept subcategory separately
    private static CraneUsageRecordDto MapToDto(CraneUsageRecord record, UsageSubcategory? subcategory)
    {
      return new CraneUsageRecordDto
      {
        Id = record.Id,
        BookingId = record.BookingId,
        BookingNumber = record.Booking?.BookingNumber,
        Date = record.Date,
        Category = record.Category,
        SubcategoryId = record.SubcategoryId,
        SubcategoryName = subcategory?.Name ?? string.Empty,
        Duration = record.Duration
      };
    }

    private static bool TryParseTimeSpan(string timeString, out TimeSpan result)
    {
      // Parse format: "HH:mm"
      if (TimeSpan.TryParseExact(timeString, @"hh\:mm", null, out result))
      {
        return true;
      }

      // Try alternative format: "H:mm"
      return TimeSpan.TryParseExact(timeString, @"h\:mm", null, out result);
    }
  }
}
