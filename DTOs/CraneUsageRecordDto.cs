// // DTOs/CraneUsageRecordDto.cs
// using AspnetCoreMvcFull.Models;

// namespace AspnetCoreMvcFull.DTOs
// {
//   public class CraneUsageRecordDto
//   {
//     public int Id { get; set; }
//     public int BookingId { get; set; }
//     public string? BookingNumber { get; set; }
//     public DateTime Date { get; set; }
//     public UsageCategory Category { get; set; }
//     public string CategoryName => Category.ToString();
//     public UsageSubcategory Subcategory { get; set; }
//     public string SubcategoryName => Subcategory.ToString();
//     public TimeSpan Duration { get; set; }
//     public string DurationFormatted => $"{Duration.Hours:D2}:{Duration.Minutes:D2}";
//   }

//   public class CraneUsageCreateDto
//   {
//     public int BookingId { get; set; }
//     public DateTime Date { get; set; }
//     public UsageCategory Category { get; set; }
//     public UsageSubcategory Subcategory { get; set; }
//     public string Duration { get; set; } = "00:00"; // Format: "HH:mm"
//   }

//   public class CraneUsageUpdateDto
//   {
//     public UsageCategory Category { get; set; }
//     public UsageSubcategory Subcategory { get; set; }
//     public string Duration { get; set; } = "00:00"; // Format: "HH:mm"
//   }

//   public class BookingUsageSummaryDto
//   {
//     public int BookingId { get; set; }
//     public string BookingNumber { get; set; } = string.Empty;
//     public DateTime Date { get; set; }
//     public List<CraneUsageRecordDto> UsageRecords { get; set; } = new List<CraneUsageRecordDto>();

//     public TimeSpan TotalOperatingTime => GetTotalTimeForCategory(UsageCategory.Operating);
//     public TimeSpan TotalDelayTime => GetTotalTimeForCategory(UsageCategory.Delay);
//     public TimeSpan TotalStandbyTime => GetTotalTimeForCategory(UsageCategory.Standby);
//     public TimeSpan TotalServiceTime => GetTotalTimeForCategory(UsageCategory.Service);
//     public TimeSpan TotalBreakdownTime => GetTotalTimeForCategory(UsageCategory.Breakdown);

//     public TimeSpan TotalAvailableTime => TotalOperatingTime + TotalDelayTime + TotalStandbyTime;
//     public TimeSpan TotalUnavailableTime => TotalServiceTime + TotalBreakdownTime;
//     public TimeSpan TotalUsageTime => TotalOperatingTime + TotalDelayTime;

//     public double AvailabilityPercentage => GetPercentage(TotalAvailableTime, TotalAvailableTime + TotalUnavailableTime);
//     public double UtilisationPercentage => GetPercentage(TotalUsageTime, TotalAvailableTime);

//     private TimeSpan GetTotalTimeForCategory(UsageCategory category)
//     {
//       return TimeSpan.FromMinutes(
//           UsageRecords
//               .Where(r => r.Category == category)
//               .Sum(r => r.Duration.TotalMinutes)
//       );
//     }

//     private double GetPercentage(TimeSpan numerator, TimeSpan denominator)
//     {
//       if (denominator.TotalMinutes == 0)
//         return 0;

//       return Math.Round((numerator.TotalMinutes / denominator.TotalMinutes) * 100, 2);
//     }
//   }
// }

using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
  public class CraneUsageRecordDto
  {
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string? BookingNumber { get; set; }
    public DateTime Date { get; set; }
    public UsageCategory Category { get; set; }
    public string CategoryName => Category.ToString();
    public int SubcategoryId { get; set; }
    public string SubcategoryName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string DurationFormatted => $"{Duration.Hours:D2}:{Duration.Minutes:D2}";
  }

  public class CraneUsageCreateDto
  {
    public int BookingId { get; set; }
    public DateTime Date { get; set; }
    public UsageCategory Category { get; set; }
    public int SubcategoryId { get; set; }
    public string Duration { get; set; } = "00:00"; // Format: "HH:mm"
  }

  public class CraneUsageUpdateDto
  {
    public UsageCategory Category { get; set; }
    public int SubcategoryId { get; set; }
    public string Duration { get; set; } = "00:00"; // Format: "HH:mm"
  }

  public class UsageSubcategoryDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public UsageCategory Category { get; set; }
  }

  public class BookingUsageSummaryDto
  {
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<CraneUsageRecordDto> UsageRecords { get; set; } = new List<CraneUsageRecordDto>();

    public TimeSpan TotalOperatingTime => GetTotalTimeForCategory(UsageCategory.Operating);
    public TimeSpan TotalDelayTime => GetTotalTimeForCategory(UsageCategory.Delay);
    public TimeSpan TotalStandbyTime => GetTotalTimeForCategory(UsageCategory.Standby);
    public TimeSpan TotalServiceTime => GetTotalTimeForCategory(UsageCategory.Service);
    public TimeSpan TotalBreakdownTime => GetTotalTimeForCategory(UsageCategory.Breakdown);

    public TimeSpan TotalAvailableTime => TotalOperatingTime + TotalDelayTime + TotalStandbyTime;
    public TimeSpan TotalUnavailableTime => TotalServiceTime + TotalBreakdownTime;
    public TimeSpan TotalUsageTime => TotalOperatingTime + TotalDelayTime;

    public double AvailabilityPercentage => GetPercentage(TotalAvailableTime, TotalAvailableTime + TotalUnavailableTime);
    public double UtilisationPercentage => GetPercentage(TotalUsageTime, TotalAvailableTime);

    private TimeSpan GetTotalTimeForCategory(UsageCategory category)
    {
      return TimeSpan.FromMinutes(
          UsageRecords
              .Where(r => r.Category == category)
              .Sum(r => r.Duration.TotalMinutes)
      );
    }

    private double GetPercentage(TimeSpan numerator, TimeSpan denominator)
    {
      if (denominator.TotalMinutes == 0)
        return 0;

      return Math.Round((numerator.TotalMinutes / denominator.TotalMinutes) * 100, 2);
    }
  }
}
