using AspnetCoreMvcFull.DTOs;

namespace AspnetCoreMvcFull.Services
{
  public interface ICraneService
  {
    Task<IEnumerable<CraneDto>> GetAllCranesAsync();
    Task<CraneDetailDto> GetCraneByIdAsync(int id);
    Task<IEnumerable<UrgentLogDto>> GetCraneUrgentLogsAsync(int id);
    Task<CraneDto> CreateCraneAsync(CraneCreateUpdateDto craneDto);
    Task UpdateCraneAsync(int id, CraneUpdateWithUrgentLogDto updateDto);
    Task DeleteCraneAsync(int id);
    Task ChangeCraneStatusToAvailableAsync(int craneId);
    Task<bool> CraneExistsAsync(int id);

    // Tambahkan metode untuk update gambar
    Task<bool> UpdateCraneImageAsync(int id, IFormFile image);
    Task RemoveCraneImageAsync(int id);
  }
}
