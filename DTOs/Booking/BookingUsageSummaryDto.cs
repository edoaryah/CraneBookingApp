using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
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
