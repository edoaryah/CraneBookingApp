using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public class UsageSubcategory
  {
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    public UsageCategory Category { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    // Opsional: untuk memudahkan migrasi dari enum
    [StringLength(100)]
    public string? OldEnumName { get; set; }
  }
}
