// // Controllers/Api/CraneUsageRecordsController.cs
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization;
// using AspnetCoreMvcFull.DTOs;
// using AspnetCoreMvcFull.Services;
// using AspnetCoreMvcFull.Filters;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using AspnetCoreMvcFull.Models;

// namespace AspnetCoreMvcFull.Controllers.Api
// {
//   [Route("api/[controller]")]
//   [ApiController]
//   [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//   [ServiceFilter(typeof(GlobalExceptionFilter))]
//   public class CraneUsageRecordsController : ControllerBase
//   {
//     private readonly ICraneUsageService _usageService;

//     public CraneUsageRecordsController(ICraneUsageService usageService)
//     {
//       _usageService = usageService;
//     }

//     // GET: api/CraneUsageRecords
//     [HttpGet]
//     public async Task<ActionResult<IEnumerable<CraneUsageRecordDto>>> GetAllUsageRecords()
//     {
//       var records = await _usageService.GetAllUsageRecordsAsync();
//       return Ok(records);
//     }

//     // GET: api/CraneUsageRecords/5
//     [HttpGet("{id}")]
//     public async Task<ActionResult<CraneUsageRecordDto>> GetUsageRecord(int id)
//     {
//       var record = await _usageService.GetUsageRecordByIdAsync(id);
//       return Ok(record);
//     }

//     // GET: api/CraneUsageRecords/Booking/5
//     [HttpGet("Booking/{bookingId}")]
//     public async Task<ActionResult<IEnumerable<CraneUsageRecordDto>>> GetUsageRecordsByBooking(int bookingId)
//     {
//       var records = await _usageService.GetUsageRecordsByBookingIdAsync(bookingId);
//       return Ok(records);
//     }

//     // GET: api/CraneUsageRecords/Summary/5
//     [HttpGet("Summary/{bookingId}")]
//     public async Task<ActionResult<BookingUsageSummaryDto>> GetBookingUsageSummary(int bookingId)
//     {
//       var summary = await _usageService.GetBookingUsageSummaryAsync(bookingId);
//       return Ok(summary);
//     }

//     // GET: api/CraneUsageRecords/Subcategories/Operating
//     [HttpGet("Subcategories/{category}")]
//     public async Task<ActionResult<IEnumerable<string>>> GetSubcategories(UsageCategory category)
//     {
//       var subcategories = await _usageService.GetSubcategoriesForCategoryAsync(category);
//       return Ok(subcategories.Select(s => s.ToString()));
//     }

//     // POST: api/CraneUsageRecords
//     [HttpPost]
//     public async Task<ActionResult<CraneUsageRecordDto>> CreateUsageRecord(CraneUsageCreateDto recordDto)
//     {
//       var result = await _usageService.CreateUsageRecordAsync(recordDto);
//       return CreatedAtAction(nameof(GetUsageRecord), new { id = result.Id }, result);
//     }

//     // PUT: api/CraneUsageRecords/5
//     [HttpPut("{id}")]
//     public async Task<ActionResult<CraneUsageRecordDto>> UpdateUsageRecord(int id, CraneUsageUpdateDto recordDto)
//     {
//       var result = await _usageService.UpdateUsageRecordAsync(id, recordDto);
//       return Ok(result);
//     }

//     // DELETE: api/CraneUsageRecords/5
//     [HttpDelete("{id}")]
//     public async Task<IActionResult> DeleteUsageRecord(int id)
//     {
//       await _usageService.DeleteUsageRecordAsync(id);
//       return NoContent();
//     }
//   }
// }

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ServiceFilter(typeof(GlobalExceptionFilter))]
  public class CraneUsageRecordsController : ControllerBase
  {
    private readonly ICraneUsageService _usageService;

    public CraneUsageRecordsController(ICraneUsageService usageService)
    {
      _usageService = usageService;
    }

    // GET: api/CraneUsageRecords
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CraneUsageRecordDto>>> GetAllUsageRecords()
    {
      var records = await _usageService.GetAllUsageRecordsAsync();
      return Ok(records);
    }

    // GET: api/CraneUsageRecords/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CraneUsageRecordDto>> GetUsageRecord(int id)
    {
      var record = await _usageService.GetUsageRecordByIdAsync(id);
      return Ok(record);
    }

    // GET: api/CraneUsageRecords/Booking/5
    [HttpGet("Booking/{bookingId}")]
    public async Task<ActionResult<IEnumerable<CraneUsageRecordDto>>> GetUsageRecordsByBooking(int bookingId)
    {
      var records = await _usageService.GetUsageRecordsByBookingIdAsync(bookingId);
      return Ok(records);
    }

    // GET: api/CraneUsageRecords/Summary/5
    [HttpGet("Summary/{bookingId}")]
    public async Task<ActionResult<BookingUsageSummaryDto>> GetBookingUsageSummary(int bookingId)
    {
      var summary = await _usageService.GetBookingUsageSummaryAsync(bookingId);
      return Ok(summary);
    }

    // GET: api/CraneUsageRecords/Subcategories/Operating
    [HttpGet("Subcategories/{category}")]
    public async Task<ActionResult<IEnumerable<UsageSubcategoryDto>>> GetSubcategories(UsageCategory category)
    {
      var subcategories = await _usageService.GetSubcategoriesForCategoryAsync(category);
      return Ok(subcategories);
    }

    // POST: api/CraneUsageRecords
    [HttpPost]
    public async Task<ActionResult<CraneUsageRecordDto>> CreateUsageRecord(CraneUsageCreateDto recordDto)
    {
      var result = await _usageService.CreateUsageRecordAsync(recordDto);
      return CreatedAtAction(nameof(GetUsageRecord), new { id = result.Id }, result);
    }

    // PUT: api/CraneUsageRecords/5
    [HttpPut("{id}")]
    public async Task<ActionResult<CraneUsageRecordDto>> UpdateUsageRecord(int id, CraneUsageUpdateDto recordDto)
    {
      var result = await _usageService.UpdateUsageRecordAsync(id, recordDto);
      return Ok(result);
    }

    // DELETE: api/CraneUsageRecords/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUsageRecord(int id)
    {
      await _usageService.DeleteUsageRecordAsync(id);
      return NoContent();
    }
  }
}
