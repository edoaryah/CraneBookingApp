using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
  // DTO untuk informasi metrik keseluruhan crane
  public class CraneMetricsDto
  {
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CraneMetricItemDto> CraneMetrics { get; set; } = new List<CraneMetricItemDto>();
    public SummaryMetricDto OverallMetrics { get; set; } = new SummaryMetricDto();
  }

  // DTO untuk metrik per crane
  public class CraneMetricItemDto
  {
    public int CraneId { get; set; }
    public string? CraneCode { get; set; }
    public int Capacity { get; set; }
    public CraneStatus Status { get; set; }

    // Metrik utama
    public MetricValuesDto Metrics { get; set; } = new MetricValuesDto();

    // Data waktu (dalam jam)
    public TimeBreakdownDto TimeBreakdown { get; set; } = new TimeBreakdownDto();
  }

  // DTO untuk nilai metrik (persentase)
  public class MetricValuesDto
  {
    public double Availability { get; set; } // Ketersediaan - Available Time / Calendar Time
    public double Utilisation { get; set; } // Pemanfaatan - Operating / Calendar Time
    public double Usage { get; set; } // Penggunaan - Utilized Time / Available Time
  }

  // DTO untuk ringkasan metrik seluruh crane
  public class SummaryMetricDto
  {
    public double AverageAvailability { get; set; }
    public double AverageUtilisation { get; set; }
    public double AverageUsage { get; set; }
    public int TotalCranes { get; set; }
    public int AvailableCranes { get; set; }
    public int MaintenanceCranes { get; set; }
  }

  // DTO untuk perincian waktu (dalam jam)
  public class TimeBreakdownDto
  {
    // Calendar Time = Available Time + Maintenance Time
    public double CalendarTime { get; set; }

    // Available Time = Utilized Time + Standby
    public double AvailableTime { get; set; }

    // Utilized Time = Operating + Delay
    public double UtilizedTime { get; set; }

    // Kategori waktu
    public double OperatingTime { get; set; }
    public double DelayTime { get; set; }
    public double StandbyTime { get; set; }
    public double ServiceTime { get; set; }
    public double BreakdownTime { get; set; }
  }

  // DTO untuk filter dashboard
  public class DashboardFilterDto
  {
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CraneId { get; set; }
  }
}
