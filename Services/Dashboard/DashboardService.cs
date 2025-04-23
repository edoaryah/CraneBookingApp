using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public class DashboardService : IDashboardService
  {
    private readonly AppDbContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(AppDbContext context, ILogger<DashboardService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<CraneMetricsDto> GetCraneMetricsAsync(DateTime? startDate = null, DateTime? endDate = null, int? craneId = null)
    {
      try
      {
        // Default: Jika startDate tidak disediakan, gunakan 30 hari terakhir
        DateTime start = startDate?.Date ?? DateTime.Now.Date.AddDays(-30);

        // Default: Jika endDate tidak disediakan, gunakan hari ini
        DateTime end = endDate?.Date ?? DateTime.Now.Date;

        _logger.LogInformation("Fetching crane metrics from {Start} to {End}", start, end);

        // Query untuk mendapatkan semua crane
        var craneQuery = _context.Cranes.AsQueryable();

        // Filter by craneId jika disediakan
        if (craneId.HasValue)
        {
          craneQuery = craneQuery.Where(c => c.Id == craneId.Value);
        }

        // Eksekusi query crane
        var cranes = await craneQuery.OrderBy(c => c.Code).ToListAsync();

        // Prepare response
        var response = new CraneMetricsDto
        {
          StartDate = start,
          EndDate = end,
          CraneMetrics = new List<CraneMetricItemDto>(),
          OverallMetrics = new SummaryMetricDto
          {
            TotalCranes = cranes.Count,
            AvailableCranes = cranes.Count(c => c.Status == CraneStatus.Available),
            MaintenanceCranes = cranes.Count(c => c.Status == CraneStatus.Maintenance)
          }
        };

        // Hitung total hari dalam periode
        double totalDays = (end - start).TotalDays + 1; // Inklusif
        double totalHours = totalDays * 24; // Total jam dalam periode

        // Untuk setiap crane, hitung metrik
        foreach (var crane in cranes)
        {
          // Dapatkan data breakdown untuk crane ini dalam periode
          var breakdowns = await _context.Breakdowns
              .Where(b => b.CraneId == crane.Id &&
                        ((b.UrgentStartTime.Date <= end &&
                          (b.ActualUrgentEndTime.HasValue ? b.ActualUrgentEndTime.Value.Date >= start : b.UrgentEndTime.Date >= start))))
              .ToListAsync();

          // Dapatkan data service (maintenance schedule) untuk crane ini dalam periode
          var maintenanceShifts = await _context.MaintenanceScheduleShifts
              .Include(ms => ms.MaintenanceSchedule)
              .Include(ms => ms.ShiftDefinition)
              .Where(ms => ms.MaintenanceSchedule.CraneId == crane.Id &&
                          ms.Date.Date >= start && ms.Date.Date <= end)
              .ToListAsync();

          // Dapatkan semua booking untuk crane ini dalam periode
          var bookingShifts = await _context.BookingShifts
              .Include(bs => bs.Booking)
              .Include(bs => bs.ShiftDefinition)
              .Where(bs => bs.Booking.CraneId == crane.Id &&
                          bs.Date.Date >= start && bs.Date.Date <= end)
              .ToListAsync();

          // Dapatkan records penggunaan crane
          var usageRecords = await _context.CraneUsageRecords
              .Include(r => r.Booking)
              .Where(r => r.Booking.CraneId == crane.Id &&
                        r.Date.Date >= start && r.Date.Date <= end)
              .ToListAsync();

          // Hitung berbagai waktu
          double breakdownHours = CalculateBreakdownHours(breakdowns, start, end);
          double serviceHours = CalculateServiceHours(maintenanceShifts);
          double standbyHours = CalculateStandbyHours(bookingShifts, usageRecords);

          // Hitung waktu dari usage records
          var usageTimeByCategory = usageRecords.GroupBy(r => r.Category)
              .ToDictionary(g => g.Key, g => g.Sum(r => r.Duration.TotalHours));

          // Dapatkan Operating dan Delay time dari records
          double operatingHours = usageTimeByCategory.ContainsKey(UsageCategory.Operating)
              ? usageTimeByCategory[UsageCategory.Operating] : 0;

          double delayHours = usageTimeByCategory.ContainsKey(UsageCategory.Delay)
              ? usageTimeByCategory[UsageCategory.Delay] : 0;

          // Jika ada waktu Breakdown dari usage records, tambahkan ke breakdownHours
          if (usageTimeByCategory.ContainsKey(UsageCategory.Breakdown))
          {
            breakdownHours += usageTimeByCategory[UsageCategory.Breakdown];
          }

          // Jika ada waktu Service dari usage records, tambahkan ke serviceHours
          if (usageTimeByCategory.ContainsKey(UsageCategory.Service))
          {
            serviceHours += usageTimeByCategory[UsageCategory.Service];
          }

          // Jika ada waktu Standby dari usage records, tambahkan ke standbyHours
          if (usageTimeByCategory.ContainsKey(UsageCategory.Standby))
          {
            standbyHours += usageTimeByCategory[UsageCategory.Standby];
          }

          // Hitung waktu-waktu turunan
          double maintenanceHours = serviceHours + breakdownHours;
          double utilizedHours = operatingHours + delayHours;
          double availableHours = totalHours - maintenanceHours;

          // Cegah pembagian dengan nol
          if (availableHours < 0) availableHours = 0;

          // Hitung metrik
          double availability = totalHours > 0 ? availableHours / totalHours * 100 : 0;
          double utilisation = totalHours > 0 ? operatingHours / totalHours * 100 : 0;
          double usage = availableHours > 0 ? utilizedHours / availableHours * 100 : 0;

          // Buat objek metrik crane
          var craneMetric = new CraneMetricItemDto
          {
            CraneId = crane.Id,
            CraneCode = crane.Code,
            Capacity = crane.Capacity,
            Status = crane.Status,
            Metrics = new MetricValuesDto
            {
              Availability = Math.Round(availability, 2),
              Utilisation = Math.Round(utilisation, 2),
              Usage = Math.Round(usage, 2)
            },
            TimeBreakdown = new TimeBreakdownDto
            {
              CalendarTime = Math.Round(totalHours, 2),
              AvailableTime = Math.Round(availableHours, 2),
              UtilizedTime = Math.Round(utilizedHours, 2),
              OperatingTime = Math.Round(operatingHours, 2),
              DelayTime = Math.Round(delayHours, 2),
              StandbyTime = Math.Round(standbyHours, 2),
              ServiceTime = Math.Round(serviceHours, 2),
              BreakdownTime = Math.Round(breakdownHours, 2)
            }
          };

          response.CraneMetrics.Add(craneMetric);
        }

        // Hitung overall metrics (rata-rata)
        if (response.CraneMetrics.Any())
        {
          response.OverallMetrics.AverageAvailability = Math.Round(response.CraneMetrics.Average(m => m.Metrics.Availability), 2);
          response.OverallMetrics.AverageUtilisation = Math.Round(response.CraneMetrics.Average(m => m.Metrics.Utilisation), 2);
          response.OverallMetrics.AverageUsage = Math.Round(response.CraneMetrics.Average(m => m.Metrics.Usage), 2);
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error calculating crane metrics: {Message}", ex.Message);
        throw;
      }
    }

    #region Helper Methods

    private double CalculateBreakdownHours(List<Breakdown> breakdowns, DateTime start, DateTime end)
    {
      double totalHours = 0;

      foreach (var breakdown in breakdowns)
      {
        // Tentukan waktu awal dan akhir yang berada dalam periode
        DateTime effectiveStart = breakdown.UrgentStartTime.Date < start ? start : breakdown.UrgentStartTime;

        DateTime effectiveEnd;
        if (breakdown.ActualUrgentEndTime.HasValue)
        {
          effectiveEnd = breakdown.ActualUrgentEndTime.Value.Date > end ? end.AddDays(1).AddSeconds(-1) : breakdown.ActualUrgentEndTime.Value;
        }
        else
        {
          effectiveEnd = breakdown.UrgentEndTime.Date > end ? end.AddDays(1).AddSeconds(-1) : breakdown.UrgentEndTime;
        }

        // Hitung durasi
        var duration = effectiveEnd - effectiveStart;
        totalHours += duration.TotalHours;
      }

      return totalHours;
    }

    private double CalculateServiceHours(List<MaintenanceScheduleShift> maintenanceShifts)
    {
      double totalHours = 0;

      foreach (var shift in maintenanceShifts)
      {
        // Durasi shift
        TimeSpan startTime = shift.ShiftStartTime != default ? shift.ShiftStartTime : shift.ShiftDefinition?.StartTime ?? TimeSpan.Zero;
        TimeSpan endTime = shift.ShiftEndTime != default ? shift.ShiftEndTime : shift.ShiftDefinition?.EndTime ?? TimeSpan.Zero;

        double shiftHours;
        if (endTime < startTime) // Shift melewati tengah malam
        {
          shiftHours = (24 - startTime.TotalHours) + endTime.TotalHours;
        }
        else
        {
          shiftHours = endTime.TotalHours - startTime.TotalHours;
        }

        totalHours += shiftHours;
      }

      return totalHours;
    }

    private double CalculateStandbyHours(List<BookingShift> bookingShifts, List<CraneUsageRecord> usageRecords)
    {
      // Hitung total hours dari booking shifts
      double totalBookedHours = 0;

      foreach (var shift in bookingShifts)
      {
        // Durasi shift
        TimeSpan startTime = shift.ShiftStartTime != default ? shift.ShiftStartTime : shift.ShiftDefinition?.StartTime ?? TimeSpan.Zero;
        TimeSpan endTime = shift.ShiftEndTime != default ? shift.ShiftEndTime : shift.ShiftDefinition?.EndTime ?? TimeSpan.Zero;

        double shiftHours;
        if (endTime < startTime) // Shift melewati tengah malam
        {
          shiftHours = (24 - startTime.TotalHours) + endTime.TotalHours;
        }
        else
        {
          shiftHours = endTime.TotalHours - startTime.TotalHours;
        }

        totalBookedHours += shiftHours;
      }

      // Hitung total jam utilized (Operating + Delay) dari usage records
      double totalUtilizedHours = usageRecords
          .Where(r => r.Category == UsageCategory.Operating || r.Category == UsageCategory.Delay)
          .Sum(r => r.Duration.TotalHours);

      // Standby adalah selisih antara total booked hours dengan utilized hours
      return Math.Max(0, totalBookedHours - totalUtilizedHours);
    }

    #endregion
  }
}
