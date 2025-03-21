using Hangfire;
using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Helpers;

namespace AspnetCoreMvcFull.Services
{
  public class CraneService : ICraneService
  {
    private readonly AppDbContext _context;

    public CraneService(AppDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<CraneDto>> GetAllCranesAsync()
    {
      var cranes = await _context.Cranes.ToListAsync();

      return cranes.Select(c => new CraneDto
      {
        Id = c.Id,
        Code = c.Code,
        Capacity = c.Capacity,
        Status = c.Status
      }).ToList();
    }

    public async Task<CraneDetailDto> GetCraneByIdAsync(int id)
    {
      var crane = await _context.Cranes
          .Include(c => c.UrgentLogs)
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
        UrgentLogs = crane.UrgentLogs?.Select(ul => new UrgentLogDto
        {
          Id = ul.Id,
          CraneId = ul.CraneId,
          UrgentStartTime = TimeZoneHelper.UtcToWita(ul.UrgentStartTime),
          EstimatedUrgentDays = ul.EstimatedUrgentDays,
          EstimatedUrgentHours = ul.EstimatedUrgentHours,
          UrgentEndTime = TimeZoneHelper.UtcToWita(ul.UrgentEndTime),
          Reasons = ul.Reasons
        }).ToList() ?? new List<UrgentLogDto>()
      };

      return craneDetailDto;
    }

    public async Task<IEnumerable<UrgentLogDto>> GetCraneUrgentLogsAsync(int id)
    {
      if (!await CraneExistsAsync(id))
      {
        throw new KeyNotFoundException($"Crane with ID {id} not found");
      }

      var urgentLogs = await _context.UrgentLogs
          .Where(ul => ul.CraneId == id)
          .OrderByDescending(ul => ul.UrgentStartTime)
          .ToListAsync();

      return urgentLogs.Select(ul => new UrgentLogDto
      {
        Id = ul.Id,
        CraneId = ul.CraneId,
        UrgentStartTime = TimeZoneHelper.UtcToWita(ul.UrgentStartTime),
        EstimatedUrgentDays = ul.EstimatedUrgentDays,
        EstimatedUrgentHours = ul.EstimatedUrgentHours,
        UrgentEndTime = TimeZoneHelper.UtcToWita(ul.UrgentEndTime),
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

      _context.Cranes.Add(crane);
      await _context.SaveChangesAsync();

      return new CraneDto
      {
        Id = crane.Id,
        Code = crane.Code,
        Capacity = crane.Capacity,
        Status = crane.Status
      };
    }

    public async Task UpdateCraneAsync(int id, CraneUpdateWithUrgentLogDto updateDto)
    {
      var existingCrane = await _context.Cranes.FindAsync(id);
      if (existingCrane == null)
      {
        throw new KeyNotFoundException($"Crane with ID {id} not found");
      }

      // Update data selain status (Code, Capacity)
      existingCrane.Code = updateDto.Crane.Code;
      existingCrane.Capacity = updateDto.Crane.Capacity;

      // Jika status crane diubah menjadi Maintenance
      if (updateDto.Crane.Status.HasValue && updateDto.Crane.Status != existingCrane.Status &&
          updateDto.Crane.Status == CraneStatus.Maintenance)
      {
        existingCrane.Status = CraneStatus.Maintenance;

        // Validasi jika UrgentLogDto disediakan
        if (updateDto.UrgentLog != null)
        {
          // Validasi field wajib
          if (string.IsNullOrEmpty(updateDto.UrgentLog.Reasons))
          {
            throw new ArgumentException("Reasons is required for maintenance status");
          }

          // Validasi salah satu harus diisi
          if (updateDto.UrgentLog.EstimatedUrgentDays <= 0 && updateDto.UrgentLog.EstimatedUrgentHours <= 0)
          {
            throw new ArgumentException("Either EstimatedUrgentDays or EstimatedUrgentHours must be greater than 0");
          }

          // Konversi waktu dari WITA ke UTC jika belum dalam UTC
          var urgentStartTimeUtc = updateDto.UrgentLog.UrgentStartTime.Kind != DateTimeKind.Utc
              ? TimeZoneHelper.WitaToUtc(updateDto.UrgentLog.UrgentStartTime)
              : updateDto.UrgentLog.UrgentStartTime;

          // Buat UrgentLog baru
          var urgentLog = new UrgentLog
          {
            CraneId = existingCrane.Id,
            UrgentStartTime = urgentStartTimeUtc,
            EstimatedUrgentDays = updateDto.UrgentLog.EstimatedUrgentDays,
            EstimatedUrgentHours = updateDto.UrgentLog.EstimatedUrgentHours,
            Reasons = updateDto.UrgentLog.Reasons
          };

          // Menghitung UrgentEndTime
          urgentLog.UrgentEndTime = urgentLog.UrgentStartTime
              .AddDays(urgentLog.EstimatedUrgentDays)
              .AddHours(urgentLog.EstimatedUrgentHours);

          // Menambahkan UrgentLog ke database
          _context.UrgentLogs.Add(urgentLog);

          // Simpan perubahan untuk mendapatkan ID UrgentLog
          await _context.SaveChangesAsync();

          // Menjadwalkan BackgroundJob untuk mengubah status crane menjadi Available setelah UrgentEndTime
          BackgroundJob.Schedule(() => ChangeCraneStatusToAvailableAsync(existingCrane.Id), urgentLog.UrgentEndTime);
        }
        else
        {
          throw new ArgumentException("UrgentLog data is required when changing status to Maintenance");
        }
      }
      else
      {
        // Jika tidak mengubah status menjadi Maintenance, cukup update data crane saja
        if (updateDto.Crane.Status.HasValue)
        {
          existingCrane.Status = updateDto.Crane.Status.Value;
        }
        _context.Entry(existingCrane).State = EntityState.Modified;
        await _context.SaveChangesAsync();
      }
    }

    public async Task DeleteCraneAsync(int id)
    {
      var crane = await _context.Cranes.FindAsync(id);
      if (crane == null)
      {
        throw new KeyNotFoundException($"Crane with ID {id} not found");
      }

      // Hapus semua UrgentLogs terkait
      var relatedLogs = await _context.UrgentLogs.Where(ul => ul.CraneId == id).ToListAsync();
      _context.UrgentLogs.RemoveRange(relatedLogs);

      _context.Cranes.Remove(crane);
      await _context.SaveChangesAsync();
    }

    public async Task ChangeCraneStatusToAvailableAsync(int craneId)
    {
      var crane = await _context.Cranes.FindAsync(craneId);
      if (crane != null && crane.Status == CraneStatus.Maintenance)
      {
        crane.Status = CraneStatus.Available;
        await _context.SaveChangesAsync();
      }
    }

    public async Task<bool> CraneExistsAsync(int id)
    {
      return await _context.Cranes.AnyAsync(e => e.Id == id);
    }
  }
}
