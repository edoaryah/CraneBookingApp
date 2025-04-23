using Hangfire;
using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Events;

namespace AspnetCoreMvcFull.Services
{
  public class CraneService : ICraneService
  {
    private readonly AppDbContext _context;
    private readonly ILogger<CraneService> _logger;
    private readonly IEventPublisher _eventPublisher;
    private readonly IFileStorageService _fileStorage;
    private const string ContainerName = "cranes";

    public CraneService(AppDbContext context, ILogger<CraneService> logger, IEventPublisher eventPublisher, IFileStorageService fileStorage)
    {
      _context = context;
      _logger = logger;
      _eventPublisher = eventPublisher;
      _fileStorage = fileStorage;
    }

    public async Task<IEnumerable<CraneDto>> GetAllCranesAsync()
    {
      var cranes = await _context.Cranes
        .OrderBy(c => c.Code)
        .ToListAsync();

      return cranes.Select(c => new CraneDto
      {
        Id = c.Id,
        Code = c.Code,
        Capacity = c.Capacity,
        Status = c.Status,
        ImagePath = c.ImagePath
      }).ToList();
    }

    public async Task<CraneDetailDto> GetCraneByIdAsync(int id)
    {
      var crane = await _context.Cranes
          .Include(c => c.Breakdowns.OrderByDescending(ul => ul.UrgentStartTime))
          .FirstOrDefaultAsync(c => c.Id == id);

      if (crane == null)
      {
        throw new KeyNotFoundException($"Crane with ID {id} not found");
      }

      var craneDetailDto = new CraneDetailDto
      {
        Id = crane.Id,
        Code = crane.Code,
        Capacity = crane.Capacity,
        Status = crane.Status,
        ImagePath = crane.ImagePath,
        Breakdowns = crane.Breakdowns?.Select(ul => new BreakdownDto
        {
          Id = ul.Id,
          CraneId = ul.CraneId,
          UrgentStartTime = ul.UrgentStartTime,
          EstimatedUrgentDays = ul.EstimatedUrgentDays,
          EstimatedUrgentHours = ul.EstimatedUrgentHours,
          UrgentEndTime = ul.UrgentEndTime,
          ActualUrgentEndTime = ul.ActualUrgentEndTime,
          HangfireJobId = ul.HangfireJobId,
          Reasons = ul.Reasons
        }).ToList() ?? new List<BreakdownDto>()
      };

      return craneDetailDto;
    }

    public async Task<IEnumerable<BreakdownDto>> GetCraneBreakdownsAsync(int id)
    {
      if (!await CraneExistsAsync(id))
      {
        throw new KeyNotFoundException($"Crane with ID {id} not found");
      }

      var breakdowns = await _context.Breakdowns
          .Where(ul => ul.CraneId == id)
          .OrderByDescending(ul => ul.UrgentStartTime)
          .ToListAsync();

      return breakdowns.Select(ul => new BreakdownDto
      {
        Id = ul.Id,
        CraneId = ul.CraneId,
        UrgentStartTime = ul.UrgentStartTime,
        EstimatedUrgentDays = ul.EstimatedUrgentDays,
        EstimatedUrgentHours = ul.EstimatedUrgentHours,
        UrgentEndTime = ul.UrgentEndTime,
        ActualUrgentEndTime = ul.ActualUrgentEndTime,
        HangfireJobId = ul.HangfireJobId,
        Reasons = ul.Reasons
      }).ToList();
    }

    public async Task<CraneDto> CreateCraneAsync(CraneCreateUpdateDto craneDto)
    {
      var crane = new Crane
      {
        Code = craneDto.Code,
        Capacity = craneDto.Capacity,
        Status = craneDto.Status ?? CraneStatus.Available
      };

      // Upload gambar jika ada
      if (craneDto.Image != null)
      {
        crane.ImagePath = await _fileStorage.SaveFileAsync(craneDto.Image, ContainerName);
      }

      _context.Cranes.Add(crane);
      await _context.SaveChangesAsync();

      return new CraneDto
      {
        Id = crane.Id,
        Code = crane.Code,
        Capacity = crane.Capacity,
        Status = crane.Status,
        ImagePath = crane.ImagePath
      };
    }

    public async Task UpdateCraneAsync(int id, CraneUpdateWithBreakdownDto updateDto)
    {
      var existingCrane = await _context.Cranes
          .Include(c => c.Breakdowns.OrderByDescending(u => u.UrgentStartTime).Take(1))
          .FirstOrDefaultAsync(c => c.Id == id);

      if (existingCrane == null)
      {
        throw new KeyNotFoundException($"Crane with ID {id} not found");
      }

      // Update image if provided
      if (updateDto.Crane.Image != null && updateDto.Crane.Image.Length > 0)
      {
        // Hapus gambar lama jika ada
        if (!string.IsNullOrEmpty(existingCrane.ImagePath))
        {
          await _fileStorage.DeleteFileAsync(existingCrane.ImagePath, ContainerName);
        }

        // Upload gambar baru
        existingCrane.ImagePath = await _fileStorage.SaveFileAsync(updateDto.Crane.Image, ContainerName);
      }

      // Update data selain status (Code, Capacity)
      existingCrane.Code = updateDto.Crane.Code;
      existingCrane.Capacity = updateDto.Crane.Capacity;

      // Jika status crane diubah menjadi Maintenance
      if (updateDto.Crane.Status.HasValue && updateDto.Crane.Status != existingCrane.Status &&
          updateDto.Crane.Status == CraneStatus.Maintenance)
      {
        existingCrane.Status = CraneStatus.Maintenance;

        // Validasi jika BreakdownDto disediakan
        if (updateDto.Breakdown != null)
        {
          // Validasi field wajib
          if (string.IsNullOrEmpty(updateDto.Breakdown.Reasons))
          {
            throw new ArgumentException("Reasons is required for maintenance status");
          }

          // Validasi salah satu harus diisi
          if (updateDto.Breakdown.EstimatedUrgentDays <= 0 && updateDto.Breakdown.EstimatedUrgentHours <= 0)
          {
            throw new ArgumentException("Either EstimatedUrgentDays or EstimatedUrgentHours must be greater than 0");
          }

          // Pastikan waktu dalam UTC
          var urgentStartTime = updateDto.Breakdown.UrgentStartTime;

          // Buat Breakdown baru
          var breakdown = new Breakdown
          {
            CraneId = existingCrane.Id,
            UrgentStartTime = urgentStartTime,
            EstimatedUrgentDays = updateDto.Breakdown.EstimatedUrgentDays,
            EstimatedUrgentHours = updateDto.Breakdown.EstimatedUrgentHours,
            Reasons = updateDto.Breakdown.Reasons
          };

          // Menghitung UrgentEndTime
          breakdown.UrgentEndTime = breakdown.UrgentStartTime
              .AddDays(breakdown.EstimatedUrgentDays)
              .AddHours(breakdown.EstimatedUrgentHours);

          // Menambahkan Breakdown ke database
          _context.Breakdowns.Add(breakdown);

          // Simpan perubahan untuk mendapatkan ID Breakdown
          await _context.SaveChangesAsync();

          // Menjadwalkan BackgroundJob untuk mengubah status crane menjadi Available setelah UrgentEndTime
          string jobId = BackgroundJob.Schedule(() => ChangeCraneStatusToAvailableAsync(existingCrane.Id), breakdown.UrgentEndTime);

          // Simpan JobId ke Breakdown
          breakdown.HangfireJobId = jobId;
          await _context.SaveChangesAsync();

          // Publish event for relocation
          await _eventPublisher.PublishAsync(new CraneMaintenanceEvent
          {
            CraneId = existingCrane.Id,
            MaintenanceStartTime = breakdown.UrgentStartTime,
            MaintenanceEndTime = breakdown.UrgentEndTime,
            Reason = breakdown.Reasons
          });
        }
        else
        {
          throw new ArgumentException("Breakdown data is required when changing status to Maintenance");
        }
      }
      // Jika status crane diubah dari Maintenance ke Available secara manual
      else if (updateDto.Crane.Status.HasValue &&
               existingCrane.Status == CraneStatus.Maintenance &&
               updateDto.Crane.Status == CraneStatus.Available)
      {
        existingCrane.Status = CraneStatus.Available;

        // Jika ada Breakdown aktif, update ActualUrgentEndTime
        var latestBreakdown = existingCrane.Breakdowns.FirstOrDefault();
        if (latestBreakdown != null && latestBreakdown.ActualUrgentEndTime == null)
        {
          latestBreakdown.ActualUrgentEndTime = DateTime.Now;

          // Batalkan scheduled job jika ada JobId
          if (!string.IsNullOrEmpty(latestBreakdown.HangfireJobId))
          {
            try
            {
              BackgroundJob.Delete(latestBreakdown.HangfireJobId);
              _logger.LogInformation("Cancelled Hangfire job {JobId} for crane {CraneId}",
                  latestBreakdown.HangfireJobId, existingCrane.Id);
            }
            catch (Exception ex)
            {
              _logger.LogWarning(ex, "Failed to delete Hangfire job {JobId} for crane {CraneId}",
                  latestBreakdown.HangfireJobId, existingCrane.Id);
            }

            // Clear job ID
            latestBreakdown.HangfireJobId = null;
          }
        }
      }
      else
      {
        // Jika tidak mengubah status menjadi Maintenance, cukup update data crane saja
        if (updateDto.Crane.Status.HasValue)
        {
          existingCrane.Status = updateDto.Crane.Status.Value;
        }
      }

      _context.Entry(existingCrane).State = EntityState.Modified;
      await _context.SaveChangesAsync();
    }

    // Metode baru untuk update gambar saja
    public async Task<bool> UpdateCraneImageAsync(int id, IFormFile image)
    {
      var crane = await _context.Cranes.FindAsync(id);
      if (crane == null)
      {
        return false;
      }

      // Hapus gambar lama jika ada
      if (!string.IsNullOrEmpty(crane.ImagePath))
      {
        await _fileStorage.DeleteFileAsync(crane.ImagePath, ContainerName);
      }

      // Upload gambar baru
      crane.ImagePath = await _fileStorage.SaveFileAsync(image, ContainerName);

      _context.Entry(crane).State = EntityState.Modified;
      await _context.SaveChangesAsync();

      return true;
    }

    // Metode untuk menghapus gambar crane
    public async Task RemoveCraneImageAsync(int id)
    {
      var crane = await _context.Cranes.FindAsync(id);
      if (crane == null)
      {
        throw new KeyNotFoundException($"Crane with ID {id} not found");
      }

      crane.ImagePath = null;
      _context.Entry(crane).State = EntityState.Modified;
      await _context.SaveChangesAsync();
    }

    public async Task DeleteCraneAsync(int id)
    {
      var crane = await _context.Cranes.FindAsync(id);
      if (crane == null)
      {
        throw new KeyNotFoundException($"Crane with ID {id} not found");
      }

      // Hapus gambar jika ada
      if (!string.IsNullOrEmpty(crane.ImagePath))
      {
        await _fileStorage.DeleteFileAsync(crane.ImagePath, ContainerName);
      }

      // Hapus semua Breakdowns terkait
      var relatedLogs = await _context.Breakdowns.Where(ul => ul.CraneId == id).ToListAsync();

      // Batalkan semua job Hangfire terkait
      foreach (var log in relatedLogs.Where(l => !string.IsNullOrEmpty(l.HangfireJobId)))
      {
        try
        {
          BackgroundJob.Delete(log.HangfireJobId);
          _logger.LogInformation("Deleted Hangfire job {JobId} for crane {CraneId}", log.HangfireJobId, id);
        }
        catch (Exception ex)
        {
          _logger.LogWarning(ex, "Failed to delete Hangfire job {JobId}", log.HangfireJobId);
        }
      }

      _context.Breakdowns.RemoveRange(relatedLogs);
      _context.Cranes.Remove(crane);
      await _context.SaveChangesAsync();
    }

    public async Task ChangeCraneStatusToAvailableAsync(int craneId)
    {
      _logger.LogInformation("Executing scheduled job to change crane {CraneId} status to Available", craneId);

      var crane = await _context.Cranes
          .Include(c => c.Breakdowns.OrderByDescending(u => u.UrgentStartTime).Take(1))
          .FirstOrDefaultAsync(c => c.Id == craneId);

      if (crane != null && crane.Status == CraneStatus.Maintenance)
      {
        var latestLog = crane.Breakdowns.FirstOrDefault();

        // Jika masih belum ditandai manual selesai
        if (latestLog != null && latestLog.ActualUrgentEndTime == null)
        {
          crane.Status = CraneStatus.Available;
          latestLog.ActualUrgentEndTime = DateTime.Now;

          await _context.SaveChangesAsync();
          _logger.LogInformation("Crane {CraneId} status automatically changed to Available via Hangfire job", craneId);

          // Publish event for checking any necessary relocations after maintenance
          await _eventPublisher.PublishAsync(new CraneMaintenanceEvent
          {
            CraneId = craneId,
            MaintenanceStartTime = latestLog.UrgentStartTime,
            MaintenanceEndTime = DateTime.Now,
            Reason = latestLog.Reasons
          });
        }
        else
        {
          _logger.LogInformation("Crane {CraneId} already has ActualUrgentEndTime set, no action needed", craneId);
        }
      }
      else
      {
        _logger.LogInformation("Crane {CraneId} is not in Maintenance status or does not exist, no action needed", craneId);
      }
    }

    // Add this method to Services/CraneManagement/CraneService.cs class

    public async Task<IEnumerable<BreakdownHistoryDto>> GetAllBreakdownsAsync()
    {
      var breakdowns = await _context.Breakdowns
          .Include(b => b.Crane)
          .OrderByDescending(b => b.UrgentStartTime)
          .ToListAsync();

      return breakdowns.Select(b => new BreakdownHistoryDto
      {
        Id = b.Id,
        CraneId = b.CraneId,
        CraneCode = b.Crane?.Code ?? "Unknown",
        CraneCapacity = b.Crane?.Capacity ?? 0,
        UrgentStartTime = b.UrgentStartTime,
        UrgentEndTime = b.UrgentEndTime,
        ActualUrgentEndTime = b.ActualUrgentEndTime,
        EstimatedUrgentDays = b.EstimatedUrgentDays,
        EstimatedUrgentHours = b.EstimatedUrgentHours,
        Reasons = b.Reasons
      }).ToList();
    }

    public async Task<bool> CraneExistsAsync(int id)
    {
      return await _context.Cranes.AnyAsync(e => e.Id == id);
    }
  }
}
