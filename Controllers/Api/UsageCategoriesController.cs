using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ServiceFilter(typeof(GlobalExceptionFilter))]
  public class UsageCategoriesController : ControllerBase
  {
    private readonly IUsageCategoryService _categoryService;

    public UsageCategoriesController(IUsageCategoryService categoryService)
    {
      _categoryService = categoryService;
    }

    // GET: api/UsageCategories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
    {
      var categories = await _categoryService.GetAllCategoriesAsync();
      return Ok(categories);
    }
  }
}
