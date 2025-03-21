using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Models;

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

    public DbSet<UrgentLog> UrgentLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Konfigurasi Enum untuk PostgreSQL
      modelBuilder.Entity<Crane>()
          .Property(c => c.Status)
          .HasConversion<string>(); // Konversi Enum ke string dalam database

      // Relasi Crane dan UrgentLog
      modelBuilder.Entity<UrgentLog>()
          .HasOne(u => u.Crane)
          .WithMany(c => c.UrgentLogs)
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

      // Seed data untuk predefined hazards
      modelBuilder.Entity<Hazard>().HasData(
          new Hazard { Id = 1, Name = "Listrik Tegangan Tinggi" },
          new Hazard { Id = 2, Name = "Kondisi Tanah" },
          new Hazard { Id = 3, Name = "Bekerja di Dekat Bangunan" },
          new Hazard { Id = 4, Name = "Bekerja di Dekat Area Mining" },
          new Hazard { Id = 5, Name = "Bekerja di Dekat Air" }
      );
    }
  }
}
