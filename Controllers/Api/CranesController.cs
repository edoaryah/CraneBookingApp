using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/[controller]")]
  [ApiController]
  [ServiceFilter(typeof(GlobalExceptionFilter))]
  public class CranesController : ControllerBase
  {
    private readonly ICraneService _craneService;

    public CranesController(ICraneService craneService)
    {
      _craneService = craneService;
    }

    // GET: api/Cranes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CraneDto>>> GetCranes()
    {
      var cranes = await _craneService.GetAllCranesAsync();
      return Ok(cranes);
    }

    // GET: api/Cranes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CraneDetailDto>> GetCrane(int id)
    {
      var crane = await _craneService.GetCraneByIdAsync(id);
      return Ok(crane);
    }

    // GET: api/Cranes/5/UrgentLogs
    [HttpGet("{id}/UrgentLogs")]
    public async Task<ActionResult<IEnumerable<UrgentLogDto>>> GetCraneUrgentLogs(int id)
    {
      var urgentLogs = await _craneService.GetCraneUrgentLogsAsync(id);
      return Ok(urgentLogs);
    }

    // POST: api/Cranes
    [HttpPost]
    public async Task<ActionResult<CraneDto>> PostCrane(CraneCreateUpdateDto craneDto)
    {
      var result = await _craneService.CreateCraneAsync(craneDto);
      return CreatedAtAction(nameof(GetCrane), new { id = result.Id }, result);
    }

    // PUT: api/Cranes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCrane(int id, [FromBody] CraneUpdateWithUrgentLogDto updateDto)
    {
      await _craneService.UpdateCraneAsync(id, updateDto);
      return NoContent();
    }

    // DELETE: api/Cranes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCrane(int id)
    {
      await _craneService.DeleteCraneAsync(id);
      return NoContent();
    }

    // Untuk akses Hangfire, perlu tetap mempertahankan method ini di controller
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task ChangeCraneStatusToAvailableAsync(int craneId)
    {
      await _craneService.ChangeCraneStatusToAvailableAsync(craneId);
    }
  }
}
