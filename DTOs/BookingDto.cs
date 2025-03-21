namespace AspnetCoreMvcFull.DTOs
{
  public class BookingDto
  {
    public int Id { get; set; }
    public required string BookingNumber { get; set; }
    public required string Name { get; set; }
    public required string Department { get; set; }
    public int CraneId { get; set; }
    public string? CraneCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime SubmitTime { get; set; }
    public string? Location { get; set; }
    public string? ProjectSupervisor { get; set; }
    public string? CostCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }
  }

  public class BookingDetailDto
  {
    public int Id { get; set; }
    public required string BookingNumber { get; set; }
    public required string Name { get; set; }
    public required string Department { get; set; }
    public int CraneId { get; set; }
    public string? CraneCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime SubmitTime { get; set; }
    public string? Location { get; set; }
    public string? ProjectSupervisor { get; set; }
    public string? CostCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }
    public List<BookingShiftDto> Shifts { get; set; } = new List<BookingShiftDto>();
    public List<BookingItemDto> Items { get; set; } = new List<BookingItemDto>();
    public List<HazardDto> SelectedHazards { get; set; } = new List<HazardDto>();
    public string? CustomHazard { get; set; }
  }

  public class BookingCreateDto
  {
    public required string Name { get; set; }
    public required string Department { get; set; }
    public int CraneId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public string? ProjectSupervisor { get; set; }
    public string? CostCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }
    public List<DailyShiftSelectionDto> ShiftSelections { get; set; } = new List<DailyShiftSelectionDto>();
    public List<BookingItemCreateDto> Items { get; set; } = new List<BookingItemCreateDto>();
    public List<int>? HazardIds { get; set; } = new List<int>();
    public string? CustomHazard { get; set; }
  }

  public class BookingUpdateDto
  {
    public required string Name { get; set; }
    public required string Department { get; set; }
    public int CraneId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public string? ProjectSupervisor { get; set; }
    public string? CostCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }
    public List<DailyShiftSelectionDto> ShiftSelections { get; set; } = new List<DailyShiftSelectionDto>();
    public List<BookingItemCreateDto> Items { get; set; } = new List<BookingItemCreateDto>();
    public List<int>? HazardIds { get; set; } = new List<int>();
    public string? CustomHazard { get; set; }
  }
}
