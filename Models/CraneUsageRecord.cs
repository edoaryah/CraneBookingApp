// // Models/CraneUsageRecord.cs
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace AspnetCoreMvcFull.Models
// {
//   public enum UsageCategory
//   {
//     Operating,    // Alat digunakan dan berproduksi
//     Delay,        // Alat digunakan tetapi tidak berproduksi
//     Standby,      // Alat siap digunakan tetapi tidak digunakan
//     Service,      // Perawatan terjadwal
//     Breakdown     // Perawatan tidak terjadwal
//   }

//   public enum UsageSubcategory
//   {
//     // Operating subcategories
//     Pengangkatan,
//     MenggantungBeban,

//     // Delay subcategories
//     MenungguUser,
//     MenungguKesiapanPengangkatan,
//     MenungguPengawalan,

//     // Standby subcategories
//     TidakSedangDiperlukan,
//     TidakAdaOperator,
//     TidakAdaPengawal,
//     Istirahat,
//     GantiShift,
//     TidakBisaLewat,

//     // Service subcategories
//     ServisRutinTerjadwal,

//     // Breakdown subcategories
//     Rusak,
//     Perbaikan
//   }

//   public class CraneUsageRecord
//   {
//     [Key]
//     public int Id { get; set; }

//     [Required]
//     public int BookingId { get; set; }

//     [Required]
//     public DateTime Date { get; set; }

//     [Required]
//     public UsageCategory Category { get; set; }

//     [Required]
//     public UsageSubcategory Subcategory { get; set; }

//     [Required]
//     public TimeSpan Duration { get; set; }

//     // Navigation property
//     public virtual Booking? Booking { get; set; }
//   }
// }

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  public enum UsageCategory
  {
    Operating,    // Alat digunakan dan berproduksi
    Delay,        // Alat digunakan tetapi tidak berproduksi
    Standby,      // Alat siap digunakan tetapi tidak digunakan
    Service,      // Perawatan terjadwal
    Breakdown     // Perawatan tidak terjadwal
  }

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

  public class CraneUsageRecord
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public int BookingId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public UsageCategory Category { get; set; }

    [Required]
    public int SubcategoryId { get; set; }

    [Required]
    public TimeSpan Duration { get; set; }

    // Navigation property
    [ForeignKey("BookingId")]
    public virtual Booking? Booking { get; set; }
  }
}
