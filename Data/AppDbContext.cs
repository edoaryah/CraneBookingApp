using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Data.Seeders;

namespace AspnetCoreMvcFull.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; }

    public DbSet<BookingHazard> BookingHazards { get; set; }

    public DbSet<BookingItem> BookingItems { get; set; }

    public DbSet<BookingShift> BookingShifts { get; set; }

    public DbSet<Crane> Cranes { get; set; }

    public DbSet<Hazard> Hazards { get; set; }

    public DbSet<Breakdown> Breakdowns { get; set; }

    public DbSet<ShiftDefinition> ShiftDefinitions { get; set; }

    public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }

    public DbSet<MaintenanceScheduleShift> MaintenanceScheduleShifts { get; set; }

    public DbSet<UsageSubcategory> UsageSubcategories { get; set; }

    public DbSet<CraneUsageRecord> CraneUsageRecords { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Konfigurasi Enum untuk PostgreSQL
      modelBuilder.Entity<Crane>()
          .Property(c => c.Status)
          .HasConversion<string>(); // Konversi Enum ke string dalam database

      // Relasi Crane dan Breakdown
      modelBuilder.Entity<Breakdown>()
          .HasOne(u => u.Crane)
          .WithMany(c => c.Breakdowns)
          .HasForeignKey(u => u.CraneId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi Crane dan Booking
      modelBuilder.Entity<Booking>()
          .HasOne(r => r.Crane)
          .WithMany()
          .HasForeignKey(r => r.CraneId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi Booking dan BookingShift
      modelBuilder.Entity<BookingShift>()
          .HasOne(rs => rs.Booking)
          .WithMany(r => r.BookingShifts)
          .HasForeignKey(rs => rs.BookingId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi BookingShift dan ShiftDefinition
      modelBuilder.Entity<BookingShift>()
          .HasOne(bs => bs.ShiftDefinition)
          .WithMany(sd => sd.BookingShifts)
          .HasForeignKey(bs => bs.ShiftDefinitionId)
          .OnDelete(DeleteBehavior.Restrict); // Gunakan Restrict alih-alih Cascade

      // Relasi Booking dan BookingItem
      modelBuilder.Entity<BookingItem>()
          .HasOne(bi => bi.Booking)
          .WithMany(b => b.BookingItems)
          .HasForeignKey(bi => bi.BookingId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi BookingHazard
      modelBuilder.Entity<BookingHazard>()
          .HasOne(bh => bh.Booking)
          .WithMany(b => b.BookingHazards)
          .HasForeignKey(bh => bh.BookingId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<BookingHazard>()
          .HasOne(bh => bh.Hazard)
          .WithMany()
          .HasForeignKey(bh => bh.HazardId)
          .OnDelete(DeleteBehavior.Restrict);

      // Relasi MaintenanceSchedule dan Crane
      modelBuilder.Entity<MaintenanceSchedule>()
          .HasOne(ms => ms.Crane)
          .WithMany()
          .HasForeignKey(ms => ms.CraneId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi MaintenanceSchedule dan MaintenanceScheduleShift
      modelBuilder.Entity<MaintenanceScheduleShift>()
          .HasOne(mss => mss.MaintenanceSchedule)
          .WithMany(ms => ms.MaintenanceScheduleShifts)
          .HasForeignKey(mss => mss.MaintenanceScheduleId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi MaintenanceScheduleShift dan ShiftDefinition
      modelBuilder.Entity<MaintenanceScheduleShift>()
          .HasOne(mss => mss.ShiftDefinition)
          .WithMany()
          .HasForeignKey(mss => mss.ShiftDefinitionId)
          .OnDelete(DeleteBehavior.Restrict);

      // Data/AppDbContext.cs - Add this to OnModelCreating method
      modelBuilder.Entity<CraneUsageRecord>()
          .HasOne(cur => cur.Booking)
          .WithMany()
          .HasForeignKey(cur => cur.BookingId)
          .OnDelete(DeleteBehavior.Cascade);

      // Configure Enum conversions for UsageCategory
      modelBuilder.Entity<CraneUsageRecord>()
          .Property(cur => cur.Category)
          .HasConversion<string>();

      // Konfigurasi relasi UsageSubcategory
      modelBuilder.Entity<UsageSubcategory>()
          .Property(us => us.Category)
          .HasConversion<string>();

      // Konfigurasi relasi CraneUsageRecord dan UsageSubcategory
      // Configure Role entity
      modelBuilder.Entity<Role>(entity =>
      {
        entity.ToTable("Roles");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        entity.Property(e => e.Description).HasMaxLength(255);
        entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
        entity.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Create a unique index on Name
        entity.HasIndex(e => e.Name).IsUnique();
      });

      // Configure UserRole entity
      modelBuilder.Entity<UserRole>(entity =>
      {
        entity.ToTable("UserRoles");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.LdapUser).IsRequired().HasMaxLength(50);
        entity.Property(e => e.Notes).HasMaxLength(255);
        entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
        entity.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Create a foreign key relationship with Role
        entity.HasOne(e => e.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

        // Create a unique index on LdapUser + RoleId
        entity.HasIndex(e => new { e.LdapUser, e.RoleId }).IsUnique();
      });
      // --------------------

      // Panggil seeders
      CraneSeeder.Seed(modelBuilder);
      HazardSeeder.Seed(modelBuilder);
      ShiftDefinitionSeeder.Seed(modelBuilder);
      UsageSubcategorySeeder.Seed(modelBuilder);
    }
  }
}
