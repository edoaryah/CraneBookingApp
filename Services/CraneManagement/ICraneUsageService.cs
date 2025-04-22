using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public interface ICraneUsageService
  {
    Task<IEnumerable<CraneUsageRecordDto>> GetAllUsageRecordsAsync();
    Task<IEnumerable<CraneUsageRecordDto>> GetUsageRecordsByBookingIdAsync(int bookingId);
    Task<BookingUsageSummaryDto> GetBookingUsageSummaryAsync(int bookingId);
    Task<CraneUsageRecordDto> GetUsageRecordByIdAsync(int id);
    Task<CraneUsageRecordDto> CreateUsageRecordAsync(CraneUsageCreateDto recordDto);
    Task<CraneUsageRecordDto> UpdateUsageRecordAsync(int id, CraneUsageUpdateDto recordDto);
    Task DeleteUsageRecordAsync(int id);
    Task<bool> UsageRecordExistsAsync(int id);
    Task<IEnumerable<UsageSubcategoryDto>> GetSubcategoriesForCategoryAsync(UsageCategory category);
  }
}
