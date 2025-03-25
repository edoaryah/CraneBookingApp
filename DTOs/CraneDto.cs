using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
  public class CraneDto
  {
    public int Id { get; set; }
    public required string Code { get; set; }
    public int Capacity { get; set; }
    public CraneStatus Status { get; set; }
  }

  public class CraneDetailDto
  {
    public int Id { get; set; }
    public required string Code { get; set; }
    public int Capacity { get; set; }
    public CraneStatus Status { get; set; }
    public ICollection<UrgentLogDto> UrgentLogs { get; set; } = new List<UrgentLogDto>();
  }

  public class CraneCreateUpdateDto
  {
    public required string Code { get; set; }
    public int Capacity { get; set; }
    public CraneStatus? Status { get; set; }
  }

  // DTOs/CraneDto.cs - Update UrgentLogDto
  public class UrgentLogDto
  {
    public int Id { get; set; }
    public int CraneId { get; set; }
    public DateTime UrgentStartTime { get; set; }
    public int EstimatedUrgentDays { get; set; }
    public int EstimatedUrgentHours { get; set; }
    public DateTime UrgentEndTime { get; set; }
    public DateTime? ActualUrgentEndTime { get; set; }
    public string? HangfireJobId { get; set; }
    public required string Reasons { get; set; }
  }

  public class UrgentLogCreateDto
  {
    public DateTime UrgentStartTime { get; set; } = DateTime.UtcNow; // Ubah ke UtcNow
    public int EstimatedUrgentDays { get; set; }
    public int EstimatedUrgentHours { get; set; }
    public required string Reasons { get; set; }
  }

  public class CraneUpdateWithUrgentLogDto
  {
    public required CraneCreateUpdateDto Crane { get; set; }
    public UrgentLogCreateDto? UrgentLog { get; set; }
  }
}
