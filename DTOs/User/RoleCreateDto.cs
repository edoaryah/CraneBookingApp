namespace AspnetCoreMvcFull.DTOs
{
  public class RoleCreateDto
  {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
  }
}
