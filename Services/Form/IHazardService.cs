using AspnetCoreMvcFull.DTOs;

namespace AspnetCoreMvcFull.Services
{
  public interface IHazardService
  {
    Task<IEnumerable<HazardDto>> GetAllHazardsAsync();
    Task<HazardDto> GetHazardByIdAsync(int id);
    Task<bool> HazardExistsAsync(int id);
  }
}
