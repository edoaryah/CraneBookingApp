namespace AspnetCoreMvcFull.DTOs
{
  public class UserRoleCreateDto
  {
    public string LdapUser { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string? Notes { get; set; }
  }
}
