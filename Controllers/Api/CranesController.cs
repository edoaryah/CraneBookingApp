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
  // [Authorize]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ServiceFilter(typeof(GlobalExceptionFilter))]
  public class CranesController : ControllerBase
  {
    private readonly ICraneService _craneService;
    private readonly IFileStorageService _fileStorage;

    public CranesController(ICraneService craneService, IFileStorageService fileStorage)
    {
      _craneService = craneService;
      _fileStorage = fileStorage;
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

    // GET: api/Cranes/5/Breakdowns
    [HttpGet("{id}/Breakdowns")]
    public async Task<ActionResult<IEnumerable<BreakdownDto>>> GetCraneBreakdowns(int id)
    {
      var breakdowns = await _craneService.GetCraneBreakdownsAsync(id);
      return Ok(breakdowns);
    }

    // POST: api/Cranes
    [HttpPost]
    public async Task<ActionResult<CraneDto>> PostCrane([FromForm] CraneCreateUpdateDto craneDto)
    {
      var result = await _craneService.CreateCraneAsync(craneDto);
      return CreatedAtAction(nameof(GetCrane), new { id = result.Id }, result);
    }

    // PUT: api/Cranes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCrane(int id, [FromForm] CraneUpdateWithBreakdownDto updateDto, [FromForm] bool? removeImage = false)
    {
      try
      {
        // Jika flag removeImage diset, bersihkan gambar
        if (removeImage == true)
        {
          var existingCrane = await _craneService.GetCraneByIdAsync(id);
          if (!string.IsNullOrEmpty(existingCrane.ImagePath))
          {
            await _fileStorage.DeleteFileAsync(existingCrane.ImagePath, "cranes");
            await _craneService.RemoveCraneImageAsync(id);
          }
        }

        // Lanjutkan dengan update normal
        await _craneService.UpdateCraneAsync(id, updateDto);
        return NoContent();
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(ex.Message);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    // DELETE: api/Cranes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCrane(int id)
    {
      await _craneService.DeleteCraneAsync(id);
      return NoContent();
    }

    // Endpoint khusus untuk upload gambar crane
    [HttpPost("{id}/image")]
    public async Task<IActionResult> UploadImage(int id, IFormFile image)
    {
      if (image == null || image.Length == 0)
      {
        return BadRequest("No image file provided");
      }

      // Validasi tipe file (hanya izinkan gambar)
      var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
      var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();

      if (!allowedExtensions.Contains(fileExtension))
      {
        return BadRequest("Invalid file type. Only image files are allowed.");
      }

      // Validasi ukuran file (max 5MB)
      if (image.Length > 5 * 1024 * 1024)
      {
        return BadRequest("File size exceeds the limit (5MB).");
      }

      try
      {
        var result = await _craneService.UpdateCraneImageAsync(id, image);

        if (!result)
        {
          return NotFound($"Crane with ID {id} not found");
        }

        return NoContent();
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    // Untuk akses Hangfire, perlu tetap mempertahankan method ini di controller
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task ChangeCraneStatusToAvailableAsync(int craneId)
    {
      await _craneService.ChangeCraneStatusToAvailableAsync(craneId);
    }
  }
}
