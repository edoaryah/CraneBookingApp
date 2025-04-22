using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs;

namespace AspnetCoreMvcFull.Services
{
  public class HazardService : IHazardService
  {
    private readonly AppDbContext _context;
    private readonly ILogger<HazardService> _logger;

    public HazardService(AppDbContext context, ILogger<HazardService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<IEnumerable<HazardDto>> GetAllHazardsAsync()
    {
      var hazards = await _context.Hazards
          .OrderBy(h => h.Name)
          .ToListAsync();

      return hazards.Select(h => new HazardDto
      {
        Id = h.Id,
        Name = h.Name
      }).ToList();
    }

    public async Task<HazardDto> GetHazardByIdAsync(int id)
    {
      var hazard = await _context.Hazards
          .FirstOrDefaultAsync(h => h.Id == id);

      if (hazard == null)
      {
        throw new KeyNotFoundException($"Hazard with ID {id} not found");
      }

      return new HazardDto
      {
        Id = hazard.Id,
        Name = hazard.Name
      };
    }

    public async Task<bool> HazardExistsAsync(int id)
    {
      return await _context.Hazards.AnyAsync(h => h.Id == id);
    }
  }
}
