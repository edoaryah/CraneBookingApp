// Controllers/Api/BreakdownsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AspnetCoreMvcFull.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ServiceFilter(typeof(GlobalExceptionFilter))]
  public class BreakdownsController : ControllerBase
  {
    private readonly ICraneService _craneService;

    public BreakdownsController(ICraneService craneService)
    {
      _craneService = craneService;
    }

    // GET: api/Breakdowns
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BreakdownHistoryDto>>> GetAllBreakdowns()
    {
      var breakdowns = await _craneService.GetAllBreakdownsAsync();
      return Ok(breakdowns);
    }

    // GET: api/Breakdowns/crane/5
    [HttpGet("crane/{craneId}")]
    public async Task<ActionResult<IEnumerable<BreakdownDto>>> GetBreakdownsByCrane(int craneId)
    {
      var breakdowns = await _craneService.GetCraneBreakdownsAsync(craneId);
      return Ok(breakdowns);
    }
  }
}
