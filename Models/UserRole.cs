using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  public class Role
  {
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    // Navigation property
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
  }

  public class UserRole
  {
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string LdapUser { get; set; } = string.Empty;

    [Required]
    public int RoleId { get; set; }

    [StringLength(255)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    [ForeignKey("RoleId")]
    public Role? Role { get; set; }

    [NotMapped]
    public Employee? Employee { get; set; }
  }
}
