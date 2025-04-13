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
  public class UsageSubcategoriesController : ControllerBase
  {
    private readonly IUsageSubcategoryService _subcategoryService;

    public UsageSubcategoriesController(IUsageSubcategoryService subcategoryService)
    {
      _subcategoryService = subcategoryService;
    }

    // GET: api/UsageSubcategories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsageSubcategoryDto>>> GetAllSubcategories()
    {
      var subcategories = await _subcategoryService.GetAllSubcategoriesAsync();
      return Ok(subcategories);
    }

    // GET: api/UsageSubcategories/Category/Operating
    [HttpGet("Category/{category}")]
    public async Task<ActionResult<IEnumerable<UsageSubcategoryDto>>> GetSubcategoriesByCategory(UsageCategory category)
    {
      var subcategories = await _subcategoryService.GetSubcategoriesByCategoryAsync(category);
      return Ok(subcategories);
    }

    // GET: api/UsageSubcategories/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UsageSubcategoryDto>> GetSubcategoryById(int id)
    {
      var subcategory = await _subcategoryService.GetSubcategoryByIdAsync(id);
      return Ok(subcategory);
    }

    // POST: api/UsageSubcategories
    [HttpPost]
    public async Task<ActionResult<UsageSubcategoryDto>> CreateSubcategory([FromBody] CreateSubcategoryRequest request)
    {
      var subcategory = await _subcategoryService.CreateSubcategoryAsync(request.Name, request.Category);
      return CreatedAtAction(nameof(GetSubcategoryById), new { id = subcategory.Id }, subcategory);
    }

    // PUT: api/UsageSubcategories/5
    [HttpPut("{id}")]
    public async Task<ActionResult<UsageSubcategoryDto>> UpdateSubcategory(int id, [FromBody] UpdateSubcategoryRequest request)
    {
      var subcategory = await _subcategoryService.UpdateSubcategoryAsync(id, request.Name, request.Category);
      return Ok(subcategory);
    }

    // DELETE: api/UsageSubcategories/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateSubcategory(int id)
    {
      await _subcategoryService.DeactivateSubcategoryAsync(id);
      return NoContent();
    }
  }

  public class CreateSubcategoryRequest
  {
    public required string Name { get; set; }
    public UsageCategory Category { get; set; }
  }

  public class UpdateSubcategoryRequest
  {
    public required string Name { get; set; }
    public UsageCategory Category { get; set; }
  }
}
