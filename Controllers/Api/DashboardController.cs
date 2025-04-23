using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ServiceFilter(typeof(GlobalExceptionFilter))]
  public class DashboardController : ControllerBase
  {
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
      _dashboardService = dashboardService;
    }

    // GET: api/Dashboard/CraneMetrics
    [HttpGet("CraneMetrics")]
    public async Task<ActionResult<CraneMetricsDto>> GetCraneMetrics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? craneId = null)
    {
      var metrics = await _dashboardService.GetCraneMetricsAsync(startDate, endDate, craneId);
      return Ok(metrics);
    }
  }
}
