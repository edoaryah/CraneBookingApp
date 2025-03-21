using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/[controller]")]
  [ApiController]
  [ServiceFilter(typeof(GlobalExceptionFilter))]
  public class HazardsController : ControllerBase
  {
    private readonly IHazardService _hazardService;

    public HazardsController(IHazardService hazardService)
    {
      _hazardService = hazardService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HazardDto>>> GetAllHazards()
    {
      var hazards = await _hazardService.GetAllHazardsAsync();
      return Ok(hazards);
    }
  }
}
