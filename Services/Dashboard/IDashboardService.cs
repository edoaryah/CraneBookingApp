using AspnetCoreMvcFull.DTOs;

namespace AspnetCoreMvcFull.Services
{
  public interface IDashboardService
  {
    /// <summary>
    /// Mendapatkan metrik crane untuk dashboard
    /// </summary>
    /// <param name="startDate">Tanggal awal periode (opsional)</param>
    /// <param name="endDate">Tanggal akhir periode (opsional)</param>
    /// <param name="craneId">ID crane yang spesifik (opsional)</param>
    /// <returns>Data metrik crane</returns>
    Task<CraneMetricsDto> GetCraneMetricsAsync(DateTime? startDate = null, DateTime? endDate = null, int? craneId = null);
  }
}
