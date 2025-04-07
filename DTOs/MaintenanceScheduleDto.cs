namespace AspnetCoreMvcFull.DTOs
{
  public class MaintenanceScheduleDto
  {
    public int Id { get; set; }
    public int CraneId { get; set; }
    public string? CraneCode { get; set; }
    public required string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string CreatedBy { get; set; }
  }

  public class MaintenanceScheduleDetailDto
  {
    public int Id { get; set; }
    public int CraneId { get; set; }
    public string? CraneCode { get; set; }
    public required string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string CreatedBy { get; set; }
    public List<MaintenanceScheduleShiftDto> Shifts { get; set; } = new List<MaintenanceScheduleShiftDto>();
  }

  public class MaintenanceScheduleShiftDto
  {
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int ShiftDefinitionId { get; set; }
    public string? ShiftName { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
  }

  public class MaintenanceScheduleCreateDto
  {
    public int CraneId { get; set; }
    public required string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
    public required string CreatedBy { get; set; }
    public List<DailyShiftSelectionDto> ShiftSelections { get; set; } = new List<DailyShiftSelectionDto>();
  }

  public class MaintenanceScheduleUpdateDto
  {
    public int CraneId { get; set; }
    public required string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
    public List<DailyShiftSelectionDto> ShiftSelections { get; set; } = new List<DailyShiftSelectionDto>();
  }
}
