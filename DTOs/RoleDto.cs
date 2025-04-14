namespace AspnetCoreMvcFull.DTOs
{
  public class RoleDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public int UserCount { get; set; } // Count of users with this role
  }

  public class RoleCreateDto
  {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
  }

  public class RoleUpdateDto
  {
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
  }

  public class UserRoleDto
  {
    public int Id { get; set; }
    public string LdapUser { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Employee details
    public string? EmployeeName { get; set; }
    public string? EmployeeId { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
  }

  public class UserRoleCreateDto
  {
    public string LdapUser { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string? Notes { get; set; }
  }

  public class UserRoleUpdateDto
  {
    public string? Notes { get; set; }
  }
}
