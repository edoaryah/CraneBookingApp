using AspnetCoreMvcFull.Models;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Data.Seeders
{
  public class UsageSubcategorySeeder
  {
    public static void Seed(ModelBuilder modelBuilder)
    {
      // Operating subcategories
      modelBuilder.Entity<UsageSubcategory>().HasData(
        new UsageSubcategory { Id = 1, Name = "Pengangkatan", Category = UsageCategory.Operating, OldEnumName = "Pengangkatan" },
        new UsageSubcategory { Id = 2, Name = "Menggantung Beban", Category = UsageCategory.Operating, OldEnumName = "MenggantungBeban" }
      );

      // Delay subcategories
      modelBuilder.Entity<UsageSubcategory>().HasData(
        new UsageSubcategory { Id = 3, Name = "Menunggu User", Category = UsageCategory.Delay, OldEnumName = "MenungguUser" },
        new UsageSubcategory { Id = 4, Name = "Menunggu Kesiapan Pengangkatan", Category = UsageCategory.Delay, OldEnumName = "MenungguKesiapanPengangkatan" },
        new UsageSubcategory { Id = 5, Name = "Menunggu Pengawalan", Category = UsageCategory.Delay, OldEnumName = "MenungguPengawalan" }
      );

      // Standby subcategories
      modelBuilder.Entity<UsageSubcategory>().HasData(
        new UsageSubcategory { Id = 6, Name = "Tidak Sedang Diperlukan", Category = UsageCategory.Standby, OldEnumName = "TidakSedangDiperlukan" },
        new UsageSubcategory { Id = 7, Name = "Tidak Ada Operator", Category = UsageCategory.Standby, OldEnumName = "TidakAdaOperator" },
        new UsageSubcategory { Id = 8, Name = "Tidak Ada Pengawal", Category = UsageCategory.Standby, OldEnumName = "TidakAdaPengawal" },
        new UsageSubcategory { Id = 9, Name = "Istirahat", Category = UsageCategory.Standby, OldEnumName = "Istirahat" },
        new UsageSubcategory { Id = 10, Name = "Ganti Shift", Category = UsageCategory.Standby, OldEnumName = "GantiShift" },
        new UsageSubcategory { Id = 11, Name = "Tidak Bisa Lewat", Category = UsageCategory.Standby, OldEnumName = "TidakBisaLewat" }
      );

      // Service subcategories
      modelBuilder.Entity<UsageSubcategory>().HasData(
        new UsageSubcategory { Id = 12, Name = "Servis Rutin Terjadwal", Category = UsageCategory.Service, OldEnumName = "ServisRutinTerjadwal" }
      );

      // Breakdown subcategories
      modelBuilder.Entity<UsageSubcategory>().HasData(
        new UsageSubcategory { Id = 13, Name = "Rusak", Category = UsageCategory.Breakdown, OldEnumName = "Rusak" },
        new UsageSubcategory { Id = 14, Name = "Perbaikan", Category = UsageCategory.Breakdown, OldEnumName = "Perbaikan" }
      );
    }
  }
}
